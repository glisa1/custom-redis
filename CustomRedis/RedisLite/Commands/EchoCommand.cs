namespace RedisLite.Commands;

internal class EchoCommand : Command
{
    public EchoCommand(List<string> args)
        : base(args)
    { }
    public override int NumberOfExpectedArguments => 1;
    public override string CommandName => "ECHO";
    public override Task<object> ExecuteAsync()
    {
        return Task.FromResult((object)Arguments[0]);
    }
}
