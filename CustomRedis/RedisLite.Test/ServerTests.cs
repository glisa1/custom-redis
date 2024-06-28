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
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$7\r\nSetName\r\n$4\r\nBrad\r\n");

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);

        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
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

    [Fact]
    public async void OnSendingIncrCommand_Passes_WhenTheValueIsIncremented()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$14\r\nSetForIncrName\r\n$1\r\n1\r\n");
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\nincr\r\n$14\r\nSetForIncrName\r\n");
        const string expectedResult1 = ":2\r\n";
        const string expectedResult2 = ":3\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var incrResponse1 = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString1 = await incrResponse1.Content.ReadAsStringAsync();

        var incrResponse2 = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString2 = await incrResponse2.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedResult1, incrResponseString1);
        Assert.Equal(expectedResult2, incrResponseString2);
    }

    [Fact]
    public async void OnSendingIncrCommand_Fails_WhenTheValueIsNotInteger()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nIncrNotString\r\n$4\r\ntest\r\n");
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\nincr\r\n$13\r\nIncrNotString\r\n");
        var expectedResult = "-The value is not an integer or out of range.\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var incrResponse = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString = await incrResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedResult, incrResponseString);
    }

    [Fact]
    public async void OnSendingIncrCommand_Passes_WhenTheKeyPreviouslyNotExists()
    {
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\nincr\r\n$12\r\nIncrNotFound\r\n");
        var expectedResult = ":1\r\n";

        var incrResponse = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString = await incrResponse.Content.ReadAsStringAsync();

        Assert.Equal(expectedResult, incrResponseString);
    }

    [Fact]
    public async void OnSendingIncrCommand_Fails_WhenValueIsTooLarge()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$18\r\nIncrOverflowString\r\n$30\r\n234293482390480948029348230948\r\n");
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\nincr\r\n$18\r\nIncrOverflowString\r\n");
        var expectedResult = "-The value is not an integer or out of range.\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var incrResponse = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString = await incrResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedResult, incrResponseString);
    }

    [Fact]
    public async void OnSendingDecrCommand_Passes_WhenTheValueIsIncremented()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$14\r\nSetForDecrName\r\n$1\r\n3\r\n");
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\ndecr\r\n$14\r\nSetForDecrName\r\n");
        var expectedResult1 = ":2\r\n";
        const string expectedResult2 = ":1\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var incrResponse1 = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString1 = await incrResponse1.Content.ReadAsStringAsync();

        var incrResponse2 = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString2 = await incrResponse2.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedResult1, incrResponseString1);
        Assert.Equal(expectedResult2, incrResponseString2);
    }

    [Fact]
    public async void OnSendingDecrCommand_Fails_WhenTheValueIsNotInteger()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$13\r\nDecrNotString\r\n$4\r\ntest\r\n");
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\ndecr\r\n$13\r\nDecrNotString\r\n");
        const string expectedResult = "-The value is not an integer or out of range.\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var incrResponse = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString = await incrResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedResult, incrResponseString);
    }

    [Fact]
    public async void OnSendingDecrCommand_Passes_WhenTheKeyPreviouslyNotExists()
    {
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\ndecr\r\n$12\r\nDecrNotFound\r\n");
        const string expectedResult = ":-1\r\n";

        var incrResponse = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString = await incrResponse.Content.ReadAsStringAsync();

        Assert.Equal(expectedResult, incrResponseString);
    }

    [Fact]
    public async void OnSendingDecrCommand_Fails_WhenValueIsTooLarge()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$18\r\nDecrOverflowString\r\n$30\r\n234293482390480948029348230948\r\n");
        var incrHttpContent = CreateStringContent("*2\r\n$4\r\ndecr\r\n$18\r\nDecrOverflowString\r\n");
        const string expectedResult = "-The value is not an integer or out of range.\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var incrResponse = await client.PostAsync(_rediLiteAddress, incrHttpContent);
        var incrResponseString = await incrResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedResult, incrResponseString);
    }

    [Fact]
    public async void OnSendingLpushCommand_Passes_WhenValueIsAppendedToList()
    {
        var lpushHttpContent = CreateStringContent("*3\r\n$5\r\nlpush\r\n$8\r\nlpushkey\r\n$5\r\ntest1\r\n");
        var lpushHttpContent1 = CreateStringContent("*3\r\n$5\r\nlpush\r\n$8\r\nlpushkey\r\n$5\r\ntest2\r\n");
        var lpushHttpContent2 = CreateStringContent("*3\r\n$5\r\nlpush\r\n$8\r\nlpushkey\r\n$5\r\ntest3\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$8\r\nlpushkey\r\n");
        const string expectedListResult = "*3\r\n$5\r\ntest3\r\n$5\r\ntest2\r\n$5\r\ntest1\r\n";

        var response = await client.PostAsync(_rediLiteAddress, lpushHttpContent);
        var lpushResponseString = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, lpushHttpContent1);
        var lpushResponseString2 = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, lpushHttpContent2);
        var lpushResponseString3 = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(":1\r\n", lpushResponseString);
        Assert.Equal(":2\r\n", lpushResponseString2);
        Assert.Equal(":3\r\n", lpushResponseString3);
        Assert.Equal(expectedListResult, getResponseString);
    }

    [Fact]
    public async void OnSendingLpushCommand_Fails_WhenValueIsNotAList()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$12\r\nlpushnotlist\r\n$4\r\nBrad\r\n");
        var lpushHttpContent = CreateStringContent("*3\r\n$5\r\nlpush\r\n$12\r\nlpushnotlist\r\n$4\r\ntest\r\n");
        const string expectedError = "-The value is not a list.\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var lpushResponse = await client.PostAsync(_rediLiteAddress, lpushHttpContent);
        var lpushResponseString = await lpushResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedError, lpushResponseString);
    }

    [Fact]
    public async void OnSendingRpushCommand_Passes_WhenValueIsAppendedToList()
    {
        var rpushHttpContent = CreateStringContent("*3\r\n$5\r\nrpush\r\n$8\r\nrpushkey\r\n$5\r\ntest1\r\n");
        var rpushHttpContent1 = CreateStringContent("*3\r\n$5\r\nrpush\r\n$8\r\nrpushkey\r\n$5\r\ntest2\r\n");
        var rpushHttpContent2 = CreateStringContent("*3\r\n$5\r\nrpush\r\n$8\r\nrpushkey\r\n$5\r\ntest3\r\n");
        var httpContentGet = CreateStringContent("*2\r\n$3\r\nget\r\n$8\r\nrpushkey\r\n");
        const string expectedListResult = "*3\r\n$5\r\ntest1\r\n$5\r\ntest2\r\n$5\r\ntest3\r\n";

        var response = await client.PostAsync(_rediLiteAddress, rpushHttpContent);
        var rpushResponseString = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, rpushHttpContent1);
        var rpushResponseString2 = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, rpushHttpContent2);
        var rpushResponseString3 = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, httpContentGet);
        var getResponseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(":1\r\n", rpushResponseString);
        Assert.Equal(":2\r\n", rpushResponseString2);
        Assert.Equal(":3\r\n", rpushResponseString3);
        Assert.Equal(expectedListResult, getResponseString);
    }

    [Fact]
    public async void OnSendingRpushCommand_Fails_WhenValueIsNotAList()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$12\r\nrpushnotlist\r\n$4\r\nBrad\r\n");
        var lpushHttpContent = CreateStringContent("*3\r\n$5\r\nrpush\r\n$12\r\nrpushnotlist\r\n$4\r\ntest\r\n");
        const string expectedError = "-The value is not a list.\r\n";

        var setResponse = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await setResponse.Content.ReadAsStringAsync();

        var lpushResponse = await client.PostAsync(_rediLiteAddress, lpushHttpContent);
        var lpushResponseString = await lpushResponse.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(expectedError, lpushResponseString);
    }

    [Fact]
    public async void SaveTest()
    {
        var setHttpContent = CreateStringContent("*3\r\n$3\r\nset\r\n$14\r\nSetNameForSave\r\n$4\r\nBrad\r\n");
        var saveHttpContent = CreateStringContent("*1\r\n$4\r\nsave\r\n");

        var response = await client.PostAsync(_rediLiteAddress, setHttpContent);
        var setResponseString = await response.Content.ReadAsStringAsync();

        response = await client.PostAsync(_rediLiteAddress, saveHttpContent);
        var saveResponseString = await response.Content.ReadAsStringAsync();

        Assert.Equal(RESPConstants.OkResponse, setResponseString);
        Assert.Equal(RESPConstants.OkResponse, saveResponseString);
    }

    private StringContent CreateStringContent(string message)
    {
        var stringPayload = JsonConvert.SerializeObject(new { message });

        return new StringContent(stringPayload, Encoding.UTF8, "application/json");
    }
}
