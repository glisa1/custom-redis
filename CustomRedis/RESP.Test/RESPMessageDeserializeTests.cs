namespace RESP.Test
{
    public class RESPMessageDeserializeTests
    {
        private readonly RESPParser _parser;

        public RESPMessageDeserializeTests()
        {
            _parser = new RESPParser();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        public void OnMessageDeserialization_Fails_WhenMessageIsNullOrEmpty(string respMessage)
        {
            Assert.Throws<ArgumentException>(() => _parser.DeserializeMessage(respMessage));
        }

        [Theory]
        [InlineData("test@test")]
        [InlineData("$test")]
        [InlineData("+test")]
        [InlineData(".test")]
        [InlineData(".*test")]
        public void OnMessageDeserialization_Fails_WhenMessageDoesNotStartWithStartCharacter(string respMessage)
        {
            Assert.Throws<ArgumentException>(() => _parser.DeserializeMessage(respMessage));
        }

        [Theory]
        [InlineData("*2\r\n+5\r\nhello\r\n$5\r\nworld\r\n")]
        [InlineData("*2\r\n$5\r\nhello\r\n+5\r\nworld\r\n")]
        [InlineData("*2\r\n$2\r\nhello\r\n+2\r\nworld\r\n")]
        [InlineData("*2\r\n$0\r\nhello\r\n+0\r\nworld\r\n")]
        public void OnMessageDeserialization_Fails_WhenMessageIsInIncorrectFormat(string respMessage)
        {
            Assert.Throws<ArgumentException>(() => _parser.DeserializeMessage(respMessage));
        }

        [Theory]
        [InlineData("*5\r\n$5\r\nhello\r\n$5\r\nworld\r\n")]
        [InlineData("*0\r\n$5\r\nhello\r\n$5\r\nworld\r\n")]
        [InlineData("*1\r\n$5\r\nhello\r\n$5\r\nworld\r\n")]
        [InlineData("*-1\r\n$5\r\nhello\r\n$5\r\nworld\r\n")]
        public void OnMessageDeserialization_Fails_WhenMessageHasIncorrectNumberOfArrayElements(string respMessage)
        {
            Assert.Throws<ArgumentException>(() => _parser.DeserializeMessage(respMessage));
        }

        [Theory]
        [InlineData("*2\r\n$4\r\nECHO\r\n$5\r\nHello world\r\n")]
        [InlineData("*2\r\n$4\r\nECHO\r\n$1\r\nHello world\r\n")]
        public void OnMessageDeserialization_Fails_WhenMessageBulkStringElementsHaveIncorrectLength(string respMessage)
        {
            Assert.Throws<ArgumentException>(() => _parser.DeserializeMessage(respMessage));
        }

        [Fact]
        public void OnMessageDeserialization_Passes_WhenMessageIsInCorrectFormat()
        {
            var message = "*2\r\n$5\r\nhello\r\n$5\r\nworld\r\n";
            var expectedResult = new List<string> { "hello", "world" };

            var result = _parser.DeserializeMessage(message);

            Assert.Equal(expectedResult, result);
        }
    }
}