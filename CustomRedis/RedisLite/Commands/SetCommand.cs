using RedisLite.Persistance;

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
        var persistanceData = new PersistanceObject(Arguments[1]);
        return PersistanceStore.SetKey(Arguments[0], persistanceData);
    }
}
