namespace RedisLite.Commands;

internal class SetExCommand : Command
{
    public SetExCommand(List<string> args)
        : base(args)
    {   
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set ex";
    public override object Execute()
    {
        throw new NotImplementedException();
        //return Persistance.SetKey(Arguments[0], Arguments[1]);
    }
}
