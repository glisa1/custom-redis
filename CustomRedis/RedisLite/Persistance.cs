using System.Collections;

namespace RedisLite;

internal static class Persistance
{
    // Let it be hashtable for now but in the future it might be better to switch to 
    // thread-safe collection such as ConcurrentDictionary, check link under
    // https://stackoverflow.com/a/20020303/888472
    private static Hashtable HashTable = new Hashtable();

    public static object SetKey(string key, object value)
    {
        try
        {
            HashTable[key] = value;
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
            var value = HashTable[key];
            if (value == null)
            {
                return null;
            }

            return value;
        }
        catch
        {
            throw;
        }
    }
}
