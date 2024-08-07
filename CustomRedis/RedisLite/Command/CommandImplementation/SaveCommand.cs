﻿using RedisLite.Persistance;
using System.Text;
using System.Text.Json;

namespace RedisLite.Command.CommandImplementation;

internal class SaveCommand : Command
{
    public SaveCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 0;

    public override string CommandName => "save";

    public async override Task<object?> ExecuteAsync()
    {
        try
        {
            var keyValuePairs = PersistanceStore.GetKeysAndValues();

            if (keyValuePairs.Count == 0)
            {
                return "OK";
            }

            await SaveToFile(keyValuePairs);

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

        var bytes = Encoding.UTF8.GetBytes(keyValuePairsJson);

        await File.WriteAllTextAsync(fileName, keyValuePairsJson);
    }
}
