using RedisLite.Command.CommandImplementation;

namespace RedisLite.Command;

public static class CommandsMapper
{
    public static Command MapToCommand(List<string> commandAndArguments)
    {
        if (commandAndArguments.Count == 0)
        {
            throw new ArgumentException(nameof(commandAndArguments));
        }

        var command = commandAndArguments.FirstOrDefault();

        if (command == null)
        {
            throw new ArgumentException(nameof(commandAndArguments), nameof(command));
        }

        var commandLower = command.ToLower();

        if (commandLower == "set" && commandAndArguments.Count > 3)
        {
            commandLower += ' ' + commandAndArguments[commandAndArguments.Count - 2].ToLower();
        }

        var commandType = CommandsMap.GetValueOrDefault(commandLower);

        if (commandType == null)
        {
            throw new Exception("Command not supported.");
        }

        var arguments = new List<string>(commandAndArguments[1..]);

        var commandInstance = Activator.CreateInstance(commandType, new object[] { arguments }) as Command;

        if (commandInstance == null)
        {
            throw new Exception("Command unknown.");
        }

        return commandInstance;
    }

    private static Dictionary<string, Type> CommandsMap => new Dictionary<string, Type>
    {
        { "ping", typeof(PingCommand) },
        { "echo", typeof(EchoCommand) },
        { "set", typeof(SetCommand) },
        { "set ex", typeof(SetExCommand) },
        { "set px", typeof(SetPxCommand) },
        { "set exat", typeof(SetExatCommand) },
        { "set pxat", typeof(SetPxatCommand) },
        { "get", typeof(GetCommand) },
        { "exists", typeof(ExistsCommand) },
        { "del", typeof(DeleteCommand) },
        { "incr", typeof(IncrementCommand) },
        { "decr", typeof(DecrementCommand) },
        { "lpush", typeof(LPushCommand) },
        { "rpush", typeof(RPushCommand) },
        { "save", typeof(SaveCommand) },
        { "load", typeof(LoadCommand) },
    };
}
