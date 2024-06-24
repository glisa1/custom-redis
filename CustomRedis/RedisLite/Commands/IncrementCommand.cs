using RedisLite.Persistance;

namespace RedisLite.Commands;

internal sealed class IncrementCommand : Command
{
    public IncrementCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "incr";

    public override object Execute()
    {
        try
        {
            var key = Arguments[0];
            var value = PersistanceStore.GetValue(key) as PersistanceObject;
            if (value == null)
            {
                PersistanceStore.SetKey(key, new PersistanceObject("1"));
                return 1;
            }

            var intValue = Convert.ToInt64(value.PersistedData);
            var result = PersistanceStore.SetKey(key, new PersistanceObject((++intValue).ToString()));

            return intValue;
        }
        catch
        {
            return new Exception("The value is not an integer or out of range.");
        }
    }
}
