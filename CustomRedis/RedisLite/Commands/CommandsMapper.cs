using System.Reflection;

namespace RedisLite.Commands;

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

        var commandType = CommandsMap.GetValueOrDefault(command.ToLower());

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
        { "get", typeof(GetCommand) },
    };
}
