namespace RedisLite.Server;

internal class ServerConfig
{
    internal string HostAddress { get; }
    internal int Port { get; }

    public ServerConfig()
    {
        HostAddress = "127.0.0.1";
        Port = 6379;
    }

    public ServerConfig(string? host, int port)
    {
        HostAddress = host ?? "127.0.0.1";
        Port = port;
    }
}
