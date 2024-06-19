namespace RedisLite.Commands;

public abstract class Command
{
    public Command(List<string> args)
    {
        if (args.Count < NumberOfExpectedArguments)
        {
            throw new ArgumentException($"Number of provided arguments is invalid. Expected {NumberOfExpectedArguments} and got {args.Count} arguments.");
        }
        Arguments = args;
    }
    public abstract int NumberOfExpectedArguments { get; }
    public abstract string CommandName { get; }
    public List<string> Arguments { get; }
    public abstract object Execute();
}
