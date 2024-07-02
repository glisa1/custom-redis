using RedisLite.Command.Utility;
using RedisLite.Persistance;

namespace RedisLite.Command.CommandImplementation;

internal sealed class ExistsCommand : Command
{
    public ExistsCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 1;

    public override string CommandName => "exists";

    public override Task<object?> ExecuteAsync()
    {
        var key = Arguments.FirstOrDefault() ?? throw new ArgumentException("Invalid key argument.");

        var value = PersistanceStore.GetValue(key);

        return value is null ? TaskFromResultMapper.MapFromResult(0) : TaskFromResultMapper.MapFromResult(1);
    }
}
