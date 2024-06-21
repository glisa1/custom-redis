using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class DecrementCommand : Command
{
    public DecrementCommand(List<string> args)
        : base(args)
    {   
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "decr";

    public override object Execute()
    {
        try
        {
            var value = PersistanceStore.GetValue(Arguments[0]) as PersistanceObject;
            if (value == null)
            {
                PersistanceStore.SetKey(Arguments[0], new PersistanceObject(-1));
                return "(integer) -1";
            }

            var intValue = Convert.ToInt64(value.PersistedData);
            var result = PersistanceStore.SetKey(Arguments[0], new PersistanceObject(--intValue));

            return $"(integer) {intValue}";
        }
        catch
        {
            return new Exception("The value is not an integer or out of range.");
        }
    }
}
