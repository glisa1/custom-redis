using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class DeleteCommand : Command
{
    public DeleteCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "del";

    public override Task<object> ExecuteAsync()
    {
        var numberOfDeletedKeys = 0;
        foreach (var argument in Arguments)
        {
            if (PersistanceStore.DeleteKey(argument))
                numberOfDeletedKeys++;
        }

        return Task.FromResult((object)numberOfDeletedKeys);
    }
}
