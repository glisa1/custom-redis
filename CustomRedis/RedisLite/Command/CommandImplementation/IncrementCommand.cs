using RedisLite.Command.Utility;
using RedisLite.Persistance;

namespace RedisLite.Command.CommandImplementation;

internal sealed class IncrementCommand : Command
{
    public IncrementCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "incr";

    public override Task<object?> ExecuteAsync()
    {
        try
        {
            var key = Arguments[0];
            var value = PersistanceStore.GetValue(key) as PersistanceObject;
            if (value == null)
            {
                PersistanceStore.SetKey(key, new PersistanceObject("1"));
                return TaskFromResultMapper.MapFromResult(1);
            }

            var intValue = Convert.ToInt64(value.PersistedData);
            var result = PersistanceStore.SetKey(key, new PersistanceObject((++intValue).ToString()));

            return TaskFromResultMapper.MapFromResult(intValue);
        }
        catch
        {
            return TaskFromResultMapper.MapFromResult(new Exception("The value is not an integer or out of range."));
        }
    }
}
