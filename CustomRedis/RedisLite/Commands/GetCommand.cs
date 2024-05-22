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
        var value = Persistance.GetValue(Arguments[0]);
        if (value == null)
        {
            return value!;
        }

        return value.ToString()!.ToCharArray();
    }
}
