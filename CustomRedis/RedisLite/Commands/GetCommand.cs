using RedisLite.Persistance;
using RESP;

namespace RedisLite.Commands;

internal class GetCommand : Command
{
    public GetCommand(List<string> args)
        :base(args)
    {
    }
    public override int NumberOfExpectedArguments => 1;
    public override string CommandName => "get";
    public override object Execute()
    {
        var value = PersistanceStore.GetValue(Arguments[0]) as PersistanceObject;
        if (value == null)
        {
            return value!;
        }

        if (value.ExpiryDate != null && value.ExpiryDate < DateTime.UtcNow)
        {
            return null;
        }

        if (value.PersistedData is ICollection<string>)
        {
            return value.PersistedData;
        }

        return value.PersistedData.ToString()!.ToCharArray();
    }
}
