﻿using Newtonsoft.Json;
using RedisLite.Commands;
using RESP;
using Serilog;
using System.Net;
using System.Text;

namespace RedisLite.Server;

public class LiteHttpServer
{
    private readonly HttpListener serverListenter;
    private readonly RESPParser respParser;
    private readonly IPAddress ipAddress;
    private readonly int port;

    public LiteHttpServer(ServerConfig config)
    {
        ipAddress = IPAddress.Parse(config.HostAddress);
        port = config.Port;

        serverListenter = new HttpListener();
        serverListenter.Prefixes.Add($"http://{ipAddress}:{port}/");

        respParser = new RESPParser();
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Log.Information($"Server started on port {port}.");
        Log.Information("Listening for requests...");

        serverListenter.Start();

        try
        {
            while (true)
            {
                var context = await serverListenter.GetContextAsync();
                Log.Information("Connection established.");
                
                await HandleClientAsync(context, cancellationToken);
            }
        }
        finally
        {
            serverListenter.Stop();
        }
    }

    private async Task HandleClientAsync(HttpListenerContext context, CancellationToken cancellationToken = default)
    {
        var request = context.Request;
        var response = context.Response;
        try
        {
            var inputMessage = await HandleInputDataAndGetMessageAsync(request, cancellationToken);

            var commandAndArguments = respParser.DeserializeMessage(inputMessage);

            var command = CommandsMapper.MapToCommand(commandAndArguments);

            var commandResult = command.Execute();

            var responseMessage = respParser.SerializeMessage(commandResult);

            await WriteResponseAsync(response, responseMessage, cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, ex.Message);
            var responseExceptionMessage = respParser.SerializeMessage(ex);
            await WriteResponseAsync(response, responseExceptionMessage, cancellationToken);
        }
    }

    private async Task<string> HandleInputDataAndGetMessageAsync(HttpListenerRequest request, CancellationToken cancellationToken = default)
    {
        var recievedData = new byte[1_024];

        var receivedDataLength = await request.InputStream.ReadAsync(recievedData, cancellationToken);

        if (receivedDataLength == 0)
        {
            throw new ArgumentException("No data received.");
        }

        var receivedData = Encoding.UTF8.GetString(recievedData, 0, receivedDataLength);

        var receivedDataObject = JsonConvert.DeserializeAnonymousType(receivedData, new { Message = string.Empty });

        if (receivedDataObject == null)
        {
            throw new ArgumentException("No data received.");
        }

        return receivedDataObject.Message;
    }

    private async Task WriteResponseAsync(HttpListenerResponse response, RESPMessage responseMessage, CancellationToken cancellationToken = default)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(responseMessage.Message);
        response.ContentLength64 = buffer.Length;

        await response.OutputStream.WriteAsync(buffer, 0, buffer.Length, cancellationToken);
    }
}
