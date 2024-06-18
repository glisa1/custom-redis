namespace RedisLite.Persistance;

internal sealed class PersistanceObject
{
    public object PersistedData { get; init; }

    public DateTime? ExpiryDate { get; init; }

    public PersistanceObject(object data)
    {
        PersistedData = data;
    }

    public PersistanceObject(object data, int seconds = 0, int miliseconds = 0)
    {
        PersistedData = data;
        ExpiryDate = DateTime.UtcNow.AddSeconds(seconds).AddMilliseconds(miliseconds);
    }

    public PersistanceObject(object data, DateTimeOffset dateTimeOffset)
    {
        PersistedData = data;
        ExpiryDate = dateTimeOffset.DateTime;
    }
}
