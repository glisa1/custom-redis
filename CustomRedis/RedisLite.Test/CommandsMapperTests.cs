using RedisLite.Command;
using RedisLite.Command.CommandImplementation;
using System.Reflection;

namespace RedisLite.Test;

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

    /// <summary>
    /// This test should fail if command has more arguments than it should.
    /// Currently the command creation will fail only if there are less
    /// arguments than expected.
    /// </summary>
    [Fact]
    public void OnCommandMapping_Passes_WhenMappingPingCommandWithInvalidNumberOfArguments()
    {
        var commandArgs = new List<string>() { "PING", "Test" };

        var command = CommandsMapper.MapToCommand(commandArgs);

        Assert.NotNull(command);
        Assert.IsType<PingCommand>(command);
    }
}