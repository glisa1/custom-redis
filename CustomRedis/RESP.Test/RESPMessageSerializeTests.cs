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
    }
}
