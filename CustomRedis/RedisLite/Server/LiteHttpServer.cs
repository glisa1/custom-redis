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

        while (true)
        {
            serverListenter.Start();

            var connection = await serverListenter.AcceptTcpClientAsync(cancellationToken);

            var networkStream = connection.GetStream();

            await WriteResponseAsync(networkStream, "Hello there!");

            connection.Close();
        }
    }

    private async Task WriteResponseAsync(NetworkStream networkStream, string message, CancellationToken cancellationToken = default)
    {
        var contentLength = Encoding.UTF8.GetByteCount(message);

        var response = $@"HTTP/1.1 200 OK
                        Content-Type: text/plain; charset=UTF-8
                        Content-Length: {contentLength}
                        {message}";
        var responseBytes = Encoding.UTF8.GetBytes(response);

        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length, cancellationToken);
    }
}
