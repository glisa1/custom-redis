using Newtonsoft.Json;
using RedisLite.Server;
using System.Net.Http.Json;
using System.Text;

namespace RedisLite.Test;

public class ServerTests
{
    private readonly HttpClient client = new HttpClient();
    private LiteHttpServer redisLiteHttpServer;
    private const string _host = "127.0.0.1";
    private const int _port = 6379;

    public ServerTests()
    {
        var serverConfig = new ServerConfig(_host, _port);

        redisLiteHttpServer = new LiteHttpServer(serverConfig);

        redisLiteHttpServer.StartAsync();
    }

    [Fact]
    public async void OnSendingPingCommad_Passes_ReceivePongResponse()
    {
        var stringPayload = JsonConvert.SerializeObject(new { message = "*1\r\n$4\r\nPING\r\n"});

        var httpContent = new StringContent(stringPayload, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"http://{_host}:{_port}/", httpContent);

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal("+PONG\r\n", responseString);
    }
}
