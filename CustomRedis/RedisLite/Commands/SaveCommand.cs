using Microsoft.Extensions.Configuration;
using RedisLite.Persistance;
using System.Text;
using System.Text.Json;
using System.Text.Unicode;

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

            Task.WaitAny(Task.FromResult(SaveToFile(keyValuePairs)));

            return "OK";
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    private async Task SaveToFile(List<KeyValuePair<string, object>> keyValuePairs)
    {
        var fileName = GetConfigurationSectionValue("saveCommandFileName");

        var keyValuePairsJson = JsonSerializer.Serialize(keyValuePairs);

        var bytes = UTF8Encoding.UTF8.GetBytes(keyValuePairsJson);

        var fileStream = File.Create(fileName);
        await fileStream.WriteAsync(bytes, 0, bytes.Length);
    }

    private string GetConfigurationSectionValue(string key)
    {
        var configurationManager = new ConfigurationManager();
        var fileName = configurationManager.GetSection(key).Value;

        return fileName ?? throw new ArgumentException("The file name is not defined.");
    }
}
