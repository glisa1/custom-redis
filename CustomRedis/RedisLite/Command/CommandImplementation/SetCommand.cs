using RedisLite.Command.Utility;
using RedisLite.Persistance;

namespace RedisLite.Command.CommandImplementation;

internal class SetCommand : Command
{
    public SetCommand(List<string> args)
        : base(args)
    {
    }
    public override int NumberOfExpectedArguments => 2;
    public override string CommandName => "set";
    public override Task<object?> ExecuteAsync()
    {
        try
        {
            var persistanceData = new PersistanceObject(Arguments[1]);
            PersistanceStore.SetKey(Arguments[0], persistanceData);

            return TaskFromResultMapper.MapFromResult("OK");
        }
        catch (Exception ex)
        {
            return TaskFromResultMapper.MapFromResult(ex);
        }
    }
}
