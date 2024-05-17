using RedisLite.Commands;
using RESP;
using Serilog;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RedisLite.Server;

internal class LiteHttpServer
{
    private readonly IPAddress ipAddress;
    private readonly int port;
    private readonly TcpListener serverListenter;

    public LiteHttpServer(ServerConfig config)
    {
        ipAddress = IPAddress.Parse(config.HostAddress);
        port = config.Port;

        serverListenter = new TcpListener(this.ipAddress, port);
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        Log.Information($"Server started on port {port}.");
        Log.Information("Listening for requests...");

        var respParser = new RESPParser();

        while (true)
        {
            serverListenter.Start();

            var connection = await serverListenter.AcceptTcpClientAsync(cancellationToken);

            var networkStream = connection.GetStream();
            var recievedData = new byte[] { };
            await networkStream.ReadAsync(recievedData, cancellationToken);

            if (recievedData.Length == 0)
            {
                var responseExceptionMessage = respParser.SerializeMessage(new Exception("Received no data"));
                await WriteResponseAsync(networkStream, responseExceptionMessage, cancellationToken);
            }

            var commandsAndArguments = respParser.DeserializeMessage(Encoding.UTF8.GetString(recievedData));

            var command = CommandsMapper.MapToCommand(commandsAndArguments);

            var commandResult = command.Execute();

            var responseMessage = respParser.SerializeMessage(commandResult);

            await WriteResponseAsync(networkStream, responseMessage, cancellationToken);

            connection.Close();
        }
    }

    private async Task WriteResponseAsync(NetworkStream networkStream, RESPMessage message, CancellationToken cancellationToken = default)
    {
        var contentLength = Encoding.UTF8.GetByteCount(message.Message);

        var response = $@"HTTP/1.1 200 OK
                        Content-Type: text/plain; charset=UTF-8
                        Content-Length: {contentLength}
                        {message.Message}";
        var responseBytes = Encoding.UTF8.GetBytes(response);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
    }
}
