using RedisLite.Persistance;
using RESP;

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
        try
        {
            var persistanceData = new PersistanceObject(Arguments[1]);
            PersistanceStore.SetKey(Arguments[0], persistanceData);

            return "OK";
        }
        catch (Exception ex)
        {
            return ex;
        }
    }
}
