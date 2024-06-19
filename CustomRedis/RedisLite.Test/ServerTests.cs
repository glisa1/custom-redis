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

        // Making sure that the data is expired.
        Thread.Sleep(10);

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.NullValues[0], getResponseString);
    }

    [Fact]
    public async void OnSendingSetExatCommand_Passes_WhenReadingTheDataNotExpired()
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddSeconds(10);
        var seconds = dateTimeOffset.ToUnixTimeSeconds();
        var secondsLength = seconds.ToString().Length;

        var httpContentCreate = CreateStringContent($"*5\r\n$3\r\nset\r\n$8\r\nTestExat\r\n$4\r\nBrad\r\n$4\r\nexat\r\n${secondsLength}\r\n{seconds}\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$8\r\nTestExat\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal("$4\r\nBrad\r\n", getResponseString);
    }

    [Fact]
    public async void OnSendingSetPxatCommand_Passes_WhenReadingTheDataNotExpired()
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddMilliseconds(10000);
        var milliseconds = dateTimeOffset.ToUnixTimeMilliseconds();
        var millisecondsLength = milliseconds.ToString().Length;

        var httpContentCreate = CreateStringContent($"*5\r\n$3\r\nset\r\n$8\r\nTestPxat\r\n$4\r\nBrad\r\n$4\r\npxat\r\n${millisecondsLength}\r\n{milliseconds}\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$8\r\nTestPxat\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal("$4\r\nBrad\r\n", getResponseString);
    }

    [Fact]
    public async void OnSendingSetExatCommand_Fails_WhenReadingTheExpiredData()
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddSeconds(1);
        var seconds = dateTimeOffset.ToUnixTimeSeconds();
        var secondsLength = seconds.ToString().Length;

        var httpContentCreate = CreateStringContent($"*5\r\n$3\r\nset\r\n$8\r\nTestExat\r\n$4\r\nBrad\r\n$4\r\nexat\r\n${secondsLength}\r\n{seconds}\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$8\r\nTestExat\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        // Making sure that the data is expired.
        Thread.Sleep(1000);

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.NullValues[0], getResponseString);
    }

    [Fact]
    public async void OnSendingSetPxatCommand_Fails_WhenReadingTheExpiredData()
    {
        DateTimeOffset dateTimeOffset = DateTimeOffset.UtcNow.AddMilliseconds(1);
        var milliseconds = dateTimeOffset.ToUnixTimeMilliseconds();
        var millisecondsLength = milliseconds.ToString().Length;

        var httpContentCreate = CreateStringContent($"*5\r\n$3\r\nset\r\n$8\r\nTestPxat\r\n$4\r\nBrad\r\n$4\r\npxat\r\n${millisecondsLength}\r\n{milliseconds}\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$8\r\nTestPxat\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var getResponse = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await getResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.NullValues[0], getResponseString);
    }

    [Fact]
    public async void OnSendingExistsCommand_Passes_WhenKeyDoesNotExist()
    {
        var httpContentExists = CreateStringContent("*2\r\n$6\r\nexists\r\n$5\r\nNoKey\r\n");

        var existsResponse = await client.PostAsync(_rediLiteAddress, httpContentExists);
        var existsResponseString = await existsResponse.Content.ReadAsStringAsync();

        Assert.Equal(":0\r\n", existsResponseString);
    }

    [Fact]
    public async void OnSendingExistsCommand_Passes_WhenKeyExist()
    {
        var httpContentCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$9\r\nKeyExists\r\n$4\r\nBrad\r\n");
        var httpContentExists = CreateStringContent("*2\r\n$6\r\nexists\r\n$9\r\nKeyExists\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var existsResponse = await client.PostAsync(_rediLiteAddress, httpContentExists);
        var existsResponseString = await existsResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(":1\r\n", existsResponseString);
    }

    [Fact]
    public async void OnSendingDeleteCommand_Passes_WhenKeyDoesNotExist()
    {
        var httpContentDelete = CreateStringContent("*2\r\n$3\r\ndel\r\n$5\r\nNoKey\r\n");

        var deleteResponse = await client.PostAsync(_rediLiteAddress, httpContentDelete);
        var deleteResponseString = await deleteResponse.Content.ReadAsStringAsync();

        Assert.Equal(":0\r\n", deleteResponseString);
    }

    [Fact]
    public async void OnSendingDeleteCommand_Passes_WhenMulitpleKeysExists()
    {
        var httpContentCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$12\r\nDelKeyExists\r\n$4\r\nBrad\r\n");
        var httpContentCreate1 = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nDelKeyExists1\r\n$4\r\nBrad\r\n");
        var httpContentCreate2 = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nDelKeyExists2\r\n$4\r\nBrad\r\n");
        var httpContentDelete = CreateStringContent("*4\r\n$3\r\ndel\r\n$12\r\nDelKeyExists\r\n$13\r\nDelKeyExists1\r\n$13\r\nDelKeyExists2\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var createResponse1 = await client.PostAsync(_rediLiteAddress, httpContentCreate1);
        var createResponseString1 = await createResponse1.Content.ReadAsStringAsync();

        var createResponse2 = await client.PostAsync(_rediLiteAddress, httpContentCreate2);
        var createResponseString2 = await createResponse2.Content.ReadAsStringAsync();

        var deleteResponse = await client.PostAsync(_rediLiteAddress, httpContentDelete);
        var deleteResponseString = await deleteResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.OkResponse, createResponseString1);
        Assert.Equal(RESPConstants.OkResponse, createResponseString2);
        Assert.Equal(":3\r\n", deleteResponseString);
    }

    [Fact]
    public async void OnSendingDeleteCommand_Passes_WhenSomeKeysExistsAndSomeDoNot()
    {
        var httpContentCreate = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nDelKeyExists4\r\n$4\r\nBrad\r\n");
        var httpContentCreate1 = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nDelKeyExists5\r\n$4\r\nBrad\r\n");
        var httpContentCreate2 = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nDelKeyExists6\r\n$4\r\nBrad\r\n");
        var httpContentDelete = CreateStringContent("*6\r\n$3\r\ndel\r\n$13\r\nDelKeyExists4\r\n$13\r\nDelKeyExists5\r\n$13\r\nDelKeyExists6\r\n$8\r\nNotFound\r\n$9\r\nNotFound1\r\n");

        var createResponse = await client.PostAsync(_rediLiteAddress, httpContentCreate);
        var createResponseString = await createResponse.Content.ReadAsStringAsync();

        var createResponse1 = await client.PostAsync(_rediLiteAddress, httpContentCreate1);
        var createResponseString1 = await createResponse1.Content.ReadAsStringAsync();

        var createResponse2 = await client.PostAsync(_rediLiteAddress, httpContentCreate2);
        var createResponseString2 = await createResponse2.Content.ReadAsStringAsync();

        var deleteResponse = await client.PostAsync(_rediLiteAddress, httpContentDelete);
        var deleteResponseString = await deleteResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, createResponseString);
        Assert.Equal(RESPConstants.OkResponse, createResponseString1);
        Assert.Equal(RESPConstants.OkResponse, createResponseString2);
        Assert.Equal(":3\r\n", deleteResponseString);
    }

    private StringContent CreateStringContent(string message)
    {
        var stringPayload = JsonConvert.SerializeObject(new { message });

        return new StringContent(stringPayload, Encoding.UTF8, "application/json");
    }
}
