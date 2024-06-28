using Microsoft.Extensions.Configuration;
using RedisLite.Persistance;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace RedisLite.Commands;

internal class SaveCommand : Command
{
    public SaveCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 0;

    public override string CommandName => "save";

    public override object Execute()
    {
        try
        {
            var keyValuePairs = PersistanceStore.GetKeysAndValues();

            if (keyValuePairs.Count == 0)
            {
                return "OK";
            }

            var result = Task.WaitAny(SaveToFile(keyValuePairs));

            return "OK";
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private async Task SaveToFile(List<KeyValuePair<string, object>> keyValuePairs)
    {
        const string fileName = "dbState.txt";

        var keyValuePairsJson = JsonSerializer.Serialize(keyValuePairs);

        var bytes = UTF8Encoding.UTF8.GetBytes(keyValuePairsJson);

        await File.WriteAllTextAsync(fileName, keyValuePairsJson);
    }
}
