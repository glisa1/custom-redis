using RedisLite.Persistance;

namespace RedisLite.Commands;

internal class SetExatCommand : Command
{
    public SetExatCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 4;
    public override string CommandName => "set eaxt";
    public override Task<object> ExecuteAsync()
    {
        try
        {
            var seconds = long.Parse(Arguments[Arguments.Count - 1]);
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(seconds);
            var persistanceData = new PersistanceObject(Arguments[1], dateTimeOffset);
            PersistanceStore.SetKey(Arguments[0], persistanceData);
            return Task.FromResult((object)"OK");
        }
        catch (Exception ex)
        {
            return Task.FromResult((object)ex);
        }
    }
}
