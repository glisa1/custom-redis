using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class RPushCommand : Command
{
    public RPushCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "rpush";

    public override object Execute()
    {
        try
        {
            var key = Arguments[0];
            var numberOfArguments = Arguments.Count - 1;
            var argumentsToAddToList = Arguments.GetRange(1, numberOfArguments);

            var value = PersistanceStore.GetValue(key) as PersistanceObject;
            if (value == null)
            {
                PersistanceStore.SetKey(key, new PersistanceObject(argumentsToAddToList));
                return numberOfArguments;
            }

            var list = (List<string>)value.PersistedData;
            list.AddRange(argumentsToAddToList);
            var result = PersistanceStore.SetKey(key, new PersistanceObject(list));

            return list.Count;
        }
        catch (Exception)
        {
            return new Exception("The value is not a list.");
        }
    }
}
