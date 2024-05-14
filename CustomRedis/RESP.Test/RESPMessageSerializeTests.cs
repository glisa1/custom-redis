using static System.Runtime.InteropServices.JavaScript.JSType;

namespace RESP.Test
{
    public class RESPMessageSerializeTests
    {
        [Theory]
        [InlineData("OK", "+OK\r\n")]
        [InlineData("hello world", "+hello world\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsSimpleStringType(string messageObject, string expectedMessage)
        {
            var serializedMessage = RESPDeserializer.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [InlineData("Error message", "-Error message\r\n")]
        [InlineData("Message in wrong format. Unexpected number of array elements.", "-Message in wrong format. Unexpected number of array elements.\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsOfErrorType(string messageObject, string expectedMessage)
        {
            var serializedMessage = RESPDeserializer.SerializeMessage(new Exception(messageObject));

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [InlineData(0, ":0\r\n")]
        [InlineData(1, ":1\r\n")]
        [InlineData(1000, ":1000\r\n")]
        [InlineData(-1, ":-1\r\n")]
        [InlineData(-1000, ":-1000\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsOfIntegerType(int messageObject, string expectedMessage)
        {
            var serializedMessage = RESPDeserializer.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [InlineData("hello", "$5\r\nhello\r\n")]
        [InlineData("world", "$5\r\nworld\r\n")]
        [InlineData("hello world", "$11\r\nhello world\r\n")]
        public void OnMessageSerialization_Passes_WhenMessageIsOfBulkStringType(string messageObject, string expectedMessage)
        {
            var serializedMessage = RESPDeserializer.SerializeBulkString(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        [Theory]
        [MemberData(nameof(ArrayTypeData))]
        public void OnMessageSerialization_Passes_WhenMessageIsOfArrayType(ICollection<object> messageObject, string expectedMessage)
        {
            var serializedMessage = RESPDeserializer.SerializeMessage(messageObject);

            Assert.Equal(serializedMessage.Message, expectedMessage);
        }

        public static IEnumerable<object[]> ArrayTypeData()
        {
            yield return new object[] { new List<object?> { "hello", "world" }, "*2\r\n$5\r\nhello\r\n$5\r\nworld\r\n" };
            yield return new object[] { new List<object?> { 1, 2, 3 }, "*3\r\n:1\r\n:2\r\n:3\r\n" };
            yield return new object[] { new List<object?> { 1, 2, 3, 4, "hello" }, "*5\r\n:1\r\n:2\r\n:3\r\n:4\r\n$5\r\nhello\r\n" };
            yield return new object[] { new List<object?> { "hello", null, "world" }, "*3\r\n$5\r\nhello\r\n$-1\r\n$5\r\nworld\r\n" };
            // The bottom option is commented out because it expects 'hello' to be simple string and currently the way to
            // differ the simple from bulk string is not implemented. Also the 'world' should be error type.
            //yield return new object[] { new List<object?> { new List<object?> { 1, 2, 3 }, new List<object?> { "hello", "world" } }, "*2\r\n*3\r\n:1\r\n:2\r\n:3\r\n*2\r\n+Hello\r\n-World\r\n" };
        }
    }
}
