namespace RedisLite.Commands;

internal class SetCommand : Command
{
    public SetCommand(List<string> args)
        :base(args)
    {
    }
    public override int NumberOfExpectedArguments => 2;
    public override string CommandName => "set";
    public override object Execute()
    {
        return Persistance.SetKey(Arguments[0], Arguments[1]);
    }
}
