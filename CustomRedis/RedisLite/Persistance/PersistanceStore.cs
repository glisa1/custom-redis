using System.Collections.Concurrent;

namespace RedisLite.Persistance;

internal static class PersistanceStore
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
        keyValuePairs.TryGetValue(key, out var value);

        return value;
    }

    public static bool DeleteKey(string key)
    {
        return keyValuePairs.TryRemove(key, out var value);
    }
}
