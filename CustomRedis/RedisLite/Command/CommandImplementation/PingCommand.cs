using RedisLite.Command.Utility;

namespace RedisLite.Command.CommandImplementation;

public class PingCommand : Command
{
    public PingCommand(List<string> args)
        : base(args)
    { }
    public override int NumberOfExpectedArguments => 0;
    public override string CommandName => "PING";
    public override Task<object?> ExecuteAsync() => TaskFromResultMapper.MapFromResult("PONG");
}
