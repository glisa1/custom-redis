﻿using RedisLite.Persistance;
using System.Text;
using System.Text.Json;

namespace RedisLite.Commands;

internal class LoadCommand : Command
{
    public LoadCommand(List<string> args)
        : base(args)
    {
    }

    public override int NumberOfExpectedArguments => 0;

    public override string CommandName => "load";

    public override object Execute()
    {
        try
        {
            List<KeyValuePair<string, object>>? keyValuePairs = null;

            Task.Run(async () => { keyValuePairs = await LoadFromFile(); }).Wait();

            if (keyValuePairs == null)
            {
                return new Exception("File could not be parsed.");
            }

            foreach (var keyValue in keyValuePairs)
            {
                PersistanceStore.SetKey(keyValue.Key, keyValue.Value);
            }

            return "OK";
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private async Task<List<KeyValuePair<string, object>>?> LoadFromFile()
    {
        const string fileName = "dbState.txt";

        var jsonKeyValues = await File.ReadAllTextAsync(fileName);

        if (jsonKeyValues == null)
        {
            throw new Exception("File could not be parsed.");
        }

        return JsonSerializer.Deserialize<List<KeyValuePair<string, object>>>(jsonKeyValues);
    }
}
