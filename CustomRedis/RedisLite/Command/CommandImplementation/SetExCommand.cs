using RedisLite.Command.Utility;
using RedisLite.Persistance;

namespace RedisLite.Command.CommandImplementation;

internal class SetExCommand : Command
{
    public SetExCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set ex";
    public override Task<object?> ExecuteAsync()
    {
        try
        {
            var persistanceData = new PersistanceObject(Arguments[1], seconds: int.Parse(Arguments[Arguments.Count - 1]));
            PersistanceStore.SetKey(Arguments[0], persistanceData);
            return TaskFromResultMapper.MapFromResult("OK");
        }
        catch (Exception ex)
        {
            return TaskFromResultMapper.MapFromResult(ex);
        }
    }
}
