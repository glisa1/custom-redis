using RedisLite.Persistance;
using RESP;
using System.Collections;

namespace RedisLite.Commands;

internal class GetCommand : Command
{
    public GetCommand(List<string> args)
        :base(args)
    {
    }
    public override int NumberOfExpectedArguments => 1;
    public override string CommandName => "get";
    public override Task<object> ExecuteAsync()
    {
        var value = PersistanceStore.GetValue(Arguments[0]) as PersistanceObject;
        if (value == null)
        {
            return Task.FromResult((object)value!);
        }

        if (value.ExpiryDate != null && value.ExpiryDate < DateTime.UtcNow)
        {
            return Task.FromResult((object)null);
        }

        if (value.PersistedData is ICollection)
        {
            return Task.FromResult((object)value.PersistedData);
        }

        return Task.FromResult((object)value.PersistedData.ToString()!.ToCharArray());
    }
}
