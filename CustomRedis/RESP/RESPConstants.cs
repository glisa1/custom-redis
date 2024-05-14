namespace RESP;

internal static class RESPConstants
{
    public const char SimpleStringType = '+';
    public const char ErrorType = '-';
    public const char IntegerType = ':';
    public const char BulkStringType = '$';
    public const char ArrayType = '*';
    public const string Terminator = "\r\n";
    public const string EmptyArray = "*0\r\n";
    public static readonly string[] NullValues = {"$-1\r\n", "*-1\r\n" };
}
