using RedisLite.Persistance;

namespace RedisLite.Commands;

internal sealed class ExistsCommand : Command
{
    public ExistsCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "exists";

    public override Task<object> ExecuteAsync()
    {
        var key = Arguments.FirstOrDefault() ?? throw new ArgumentException("Invalid key argument.");

        var value = PersistanceStore.GetValue(key);

        return value is null ? Task.FromResult((object)0) : Task.FromResult((object)1);
    }
}
