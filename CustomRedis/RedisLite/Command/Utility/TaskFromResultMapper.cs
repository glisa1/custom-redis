namespace RedisLite.Command.Utility
{
    internal static class TaskFromResultMapper
    {
        public static Task<object?> MapFromResult<T>(T value)
        {
            return Task.FromResult((object?)value);
        }
    }
}
