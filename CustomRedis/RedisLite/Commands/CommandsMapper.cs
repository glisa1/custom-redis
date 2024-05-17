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

        var commandType = CommandsMap.GetValueOrDefault(command);

        if (commandType == null)
        {
            throw new Exception("Command not supported.");
        }

        var commandInstance = Activator.CreateInstance(commandType, new object[] { commandAndArguments }) as Command;

        if (commandInstance == null) 
        {
            throw new Exception("Command unknown.");
        }

        return commandInstance;
    }

    private static Dictionary<string, Type> CommandsMap => new Dictionary<string, Type>
    { 
        { "PING", typeof(PingCommand) },
        { "ECHO", typeof(Exception) }
    };
}
