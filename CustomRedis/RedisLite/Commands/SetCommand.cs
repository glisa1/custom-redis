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
    public override Task<object> ExecuteAsync()
    {
        try
        {
            var persistanceData = new PersistanceObject(Arguments[1]);
            PersistanceStore.SetKey(Arguments[0], persistanceData);

            return Task.FromResult((object)"OK");
        }
        catch (Exception ex)
        {
            return Task.FromResult((object)ex);
        }
    }
}
