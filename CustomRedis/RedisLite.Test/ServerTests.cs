using Newtonsoft.Json;
using RedisLite.Server;
using RESP;
using System.Text;

namespace RedisLite.Test;

public class ServerTests
{
    private readonly string _rediLiteAddress = $"http://{_host}:{_port}/";
    private readonly HttpClient client = new HttpClient();
    private LiteHttpServer redisLiteHttpServer;
    private const string _host = "127.0.0.1";
    private const int _port = 6379;
    private const string wrongNumberOfParametersForCommand = "-Message in wrong format. Unexpected number of array elements.\r\n";
    private const string messageElementHasWrongLength = "-Message element has wrong length.\r\n";

    public ServerTests()
    {
        var serverConfig = new ServerConfig(_host, _port);

        redisLiteHttpServer = new LiteHttpServer(serverConfig);

        redisLiteHttpServer.StartAsync();
    }

    [Fact]
    public async void OnSendingPingCommand_Passes_ReceivePongResponse()
    {
        var httpContent = CreateStringContent("*1\r\n$4\r\nPING\r\n");

        var response = await client.PostAsync(_rediLiteAddress, httpContent);

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal("+PONG\r\n", responseString);
    }

    [Fact]
    public async void OnSendingEchoCommand_Passes_WhenSendingCorrectRequest()
    {
        var httpContent = CreateStringContent("*2\r\n$4\r\nECHO\r\n$11\r\nHello world\r\n");

        var response = await client.PostAsync(_rediLiteAddress, httpContent);

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal("+Hello world\r\n", responseString);
    }

    [Fact]
    public async void OnSendingEchoCommand_Fails_WhenNotSendingParameter()
    {
        var httpContent = CreateStringContent("*2\r\n$4\r\nECHO\r\n");

        var response = await client.PostAsync(_rediLiteAddress, httpContent);

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(wrongNumberOfParametersForCommand, responseString);
    }

    [Fact]
    public async void OnSendingGetCommand_Passes_WhenTheValueDoesNotExist()
    {
        var httpContent = CreateStringContent("*2\r\n$3\r\nget\r\n$15\r\nNameNotExisting\r\n");

        var response = await client.PostAsync(_rediLiteAddress, httpContent);

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal("$-1\r\n", responseString);
    }

    [Fact]
    public async void OnSendingSetCommand_Passes_WhenTheValueIsSaved()
    {
        var httpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$7\r\nSetName\r\n$4\r\nBrad\r\n");

        var response = await client.PostAsync(_rediLiteAddress, httpContent);

        var responseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, responseString);
    }

    [Fact]
    public async void OnSendingSetCommand_Passes_WhenTheValueExistsAndIsUpdated()
    {
        var httpContentForCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$7\r\nSetName\r\n$4\r\nBrad\r\n");
        var httpContentForUpdate = CreateStringContent("*3\r\n$3\r\nset\r\n$7\r\nSetName\r\n$4\r\nLuca\r\n");
        var httpContentForGet = CreateStringContent("*2\r\n$3\r\nget\r\n$7\r\nSetName\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentForCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var updateResponse = await client.PostAsync(_rediLiteAddress, httpContentForUpdate);
        var updateResponseString = await updateResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentForGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.OkResponse, updateResponseString);
        Assert.Equal("$4\r\nLuca\r\n", getResponseString);
    }

    [Fact]
    public async void OnSendingGetCommand_Passes_WhenTheValueExists()
    {
        var httpContentCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$4\r\nTest\r\n$4\r\nBrad\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$4\r\nTest\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal("$4\r\nBrad\r\n", getResponseString);
    }

    [Fact]
    public async void OnSendingSetCommand_Fails_WhenTheValueNotSent()
    {
        var httpContentCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$4\r\nTest\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        Assert.Equal(wrongNumberOfParametersForCommand, createResponseString);
    }

    [Fact]
    public async void OnSendingSetCommand_Fails_WhenTheValueHasWrongLength()
    {
        var httpContentCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$4\r\nTest\r\n$2\r\nBrad\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        Assert.Equal(messageElementHasWrongLength, createResponseString);
    }

    [Fact]
    public async void OnSendingSetExCommand_Passes_WhenReadingTheDataNotExpired()
    {
        var httpContentCreate = CreateStringContent("*5\r\n$3\r\nset\r\n$6\r\nTestEx\r\n$4\r\nBrad\r\n$2\r\nex\r\n$2\r\n50\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$6\r\nTestEx\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal("$4\r\nBrad\r\n", getResponseString);
    }

    [Fact]
    public async void OnSendingSetPxCommand_Passes_WhenReadingTheDataNotExpired()
    {
        var httpContentCreate = CreateStringContent("*5\r\n$3\r\nset\r\n$6\r\nTestPx\r\n$4\r\nBrad\r\n$2\r\npx\r\n$5\r\n50000\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$6\r\nTestPx\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal("$4\r\nBrad\r\n", getResponseString);
    }

    [Fact]
    public async void OnSendingSetExCommand_Fails_WhenReadingTheExpiredData()
    {
        var httpContentCreate = CreateStringContent("*5\r\n$3\r\nset\r\n$6\r\nTestEx\r\n$4\r\nBrad\r\n$2\r\nex\r\n$1\r\n1\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$6\r\nTestEx\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        //This is used to make sure that one second is passed and that the data saved is expired.
        Thread.Sleep(1000);

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.NullValues[0], getResponseString);
    }

    [Fact]
    public async void OnSendingSetPxCommand_Fails_WhenReadingTheExpiredData()
    {
        var httpContentCreate = CreateStringContent("*5\r\n$3\r\nset\r\n$6\r\nTestPx\r\n$4\r\nBrad\r\n$2\r\npx\r\n$1\r\n1\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$6\r\nTestPx\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.NullValues[0], getResponseString);
    }

    private StringContent CreateStringContent(string message)
    {
        var stringPayload = JsonConvert.SerializeObject(new { message });

        return new StringContent(stringPayload, Encoding.UTF8, "application/json");
    }
}
