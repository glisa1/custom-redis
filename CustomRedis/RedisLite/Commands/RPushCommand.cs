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

    public override Task<object> ExecuteAsync()
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
                return Task.FromResult((object)numberOfArguments);
            }

            var list = (List<string>)value.PersistedData;
            list.AddRange(argumentsToAddToList);
            var result = PersistanceStore.SetKey(key, new PersistanceObject(list));

            return Task.FromResult((object)list.Count);
        }
        catch (Exception)
        {
            return Task.FromResult((object)new Exception("The value is not a list."));
        }
    }
}
