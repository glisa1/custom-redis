namespace RESP.Test
{
    public class RESPMessageSerializeTests
    {
        private readonly RESPParser _parser;

        public RESPMessageSerializeTests()
        {
            _parser = new RESPParser();
        }

        [Theory]
        [InlineData("OK", "+OK\r\n")]
        [InlineData("hello world", "+hello world\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsOfCorrectSimpleStringType(string messageObject, string expectedMessage)
        {
            var serializedMessage = _parser.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [InlineData("Error message", "-Error message\r\n")]
        [InlineData("Message in wrong format. Unexpected number of array elements.", "-Message in wrong format. Unexpected number of array elements.\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsOfCorrectErrorType(string messageObject, string expectedMessage)
        {
            var serializedMessage = _parser.SerializeMessage(new Exception(messageObject));

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [InlineData(0, ":0\r\n")]
        [InlineData(1, ":1\r\n")]
        [InlineData(1000, ":1000\r\n")]
        [InlineData(-1, ":-1\r\n")]
        [InlineData(-1000, ":-1000\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsOfCorrectIntegerType(int messageObject, string expectedMessage)
        {
            var serializedMessage = _parser.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [MemberData(nameof(BulkStringTypeData))]
        public void OnMessageSerialization_Passes_WhenMessageIsOfCorrectBulkStringType(char[] messageObject, string expectedMessage)
        {
            var serializedMessage = _parser.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ArrayTypeData))]
        public void OnMessageSerialization_Passes_WhenMessageIsOfCorrectArrayType(ICollection<object> messageObject, string expectedMessage)
        {
            var serializedMessage = _parser.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        public static IEnumerable<object[]> BulkStringTypeData()
        {
            yield return new object[] { "hello".ToCharArray(), "$5\r\nhello\r\n" };
            yield return new object[] { "world".ToCharArray(), "$5\r\nworld\r\n" };
            yield return new object[] { "hello world".ToCharArray(), "$11\r\nhello world\r\n" };
            yield return new object[] { "wo\rrld".ToCharArray(), "$6\r\nwo\rrld\r\n" };
            yield return new object[] { "wo\nrld".ToCharArray(), "$6\r\nwo\nrld\r\n" };
            yield return new object[] { "wo\nrld".ToCharArray(), "$6\r\nwo\nrld\r\n" };
            yield return new object[] { "wo\r\nrld".ToCharArray(), "$7\r\nwo\r\nrld\r\n" };
        }

        public static IEnumerable<object[]> ArrayTypeData()
        {
            yield return new object[] { new List<object?> { "hello", "world" }, "*2\r\n$5\r\nhello\r\n$5\r\nworld\r\n" };
            yield return new object[] { new List<object?> { 1, 2, 3 }, "*3\r\n:1\r\n:2\r\n:3\r\n" };
            yield return new object[] { new List<object?> { 1, 2, 3, 4, "hello" }, "*5\r\n:1\r\n:2\r\n:3\r\n:4\r\n$5\r\nhello\r\n" };
            yield return new object[] { new List<object?> { "hello", null, "world" }, "*3\r\n$5\r\nhello\r\n$-1\r\n$5\r\nworld\r\n" };
            yield return new object[] { new List<object?> { "ping" }, "*1\r\n$4\r\nping\r\n" };
            yield return new object[] { new List<object?> { new List<object?> { 1, 2, 3 }, new List<object?> { "Hello", new Exception("World") } }, "*2\r\n*3\r\n:1\r\n:2\r\n:3\r\n*2\r\n$5\r\nHello\r\n-World\r\n" };
        }
    }
}
