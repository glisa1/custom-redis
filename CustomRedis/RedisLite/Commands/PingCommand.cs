

namespace RedisLite.Commands;

public class PingCommand : Command
{
    public PingCommand(List<string> args)
        :base(args)
    {}

    public override int NumberOfExpectedArguments => 0;

    public override string CommandName => "PING";

    public override Type ResponseType => typeof(string);

    public override object Execute()
    {
        throw new NotImplementedException();
    }
}
