using RedisLite.Command.Utility;
using RedisLite.Persistance;
using RESP;
using System.Collections;

namespace RedisLite.Command.CommandImplementation;

internal class GetCommand : Command
{
    public GetCommand(List<string> args)
        : base(args)
    {
    }
    public override int NumberOfExpectedArguments => 1;
    public override string CommandName => "get";
    public override Task<object?> ExecuteAsync()
    {
        var value = PersistanceStore.GetValue(Arguments[0]) as PersistanceObject;
        if (value == null)
        {
            return TaskFromResultMapper.MapFromResult(value!);
        }

        if (value.ExpiryDate != null && value.ExpiryDate < DateTime.UtcNow)
        {
            return TaskFromResultMapper.MapFromResult((object?)null);
        }

        if (value.PersistedData is ICollection)
        {
            return TaskFromResultMapper.MapFromResult(value.PersistedData);
        }

        return TaskFromResultMapper.MapFromResult(value.PersistedData.ToString()!.ToCharArray());
    }
}
