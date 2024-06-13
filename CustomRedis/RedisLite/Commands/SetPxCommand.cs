namespace RedisLite.Commands;

internal class SetPxCommand : Command
{
    public SetPxCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set px";
    public override object Execute()
    {
        throw new NotImplementedException();
        //return Persistance.SetKey(Arguments[0], Arguments[1]);
    }
}
