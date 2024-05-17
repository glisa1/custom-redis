using RedisLite.Commands;
using System.Reflection;

namespace RedisLite.Test
{
    public class CommandsMapperTests
    {
        [Fact]
        public void OnCommandMapping_Passes_WhenMappingPingCommand()
        {
            var commandArgs = new List<string>() { "PING" };

            var command = CommandsMapper.MapToCommand(commandArgs);

            Assert.NotNull(command);
            Assert.IsType<PingCommand>(command);
        }

        [Fact]
        public void OnCommandMapping_Fails_WhenMappingPingCommandWithZeroArguments()
        {
            var commandArgs = new List<string>();

            Assert.Throws<ArgumentException>(() => CommandsMapper.MapToCommand(commandArgs));
        }

        [Fact]
        public void OnCommandMapping_Fails_WhenMappingPingCommandWithInvalidNumberOfArguments()
        {
            var commandArgs = new List<string>() { "PING", "Test" };

            Assert.Throws<TargetInvocationException>(() => CommandsMapper.MapToCommand(commandArgs));
        }
    }
}