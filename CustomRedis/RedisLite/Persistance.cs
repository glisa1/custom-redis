using System.Collections;
using System.Collections.Concurrent;

namespace RedisLite;

internal static class Persistance
{
    private readonly static ConcurrentDictionary<string, object> keyValuePairs = new ConcurrentDictionary<string, object>();

    public static object SetKey(string key, object value)
    {
        try
        {
            keyValuePairs.AddOrUpdate(key, value, (key, oldValue) => value);
            return "OK";
        }
        catch (Exception ex)
        {
            return ex;
        }
    }

    public static object? GetValue(string key)
    {
        try
        {
            keyValuePairs.TryGetValue(key, out var value);

            return value;
        }
        catch
        {
            throw;
        }
    }
}
