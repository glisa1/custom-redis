using System.Collections;
using System.Text;

namespace RESP;

internal class RESPSerializer
{
    public RESPMessage SerializeMessage(object messageObject)
    {
        try
        {
            var messageContent = string.Empty;
            if (messageObject == null)
                messageContent = SerializeNull();
            else if (messageObject is string stringValue)
                messageContent = SerializeSimpleStringType(stringValue);
            else if (messageObject is char[] charArrayValue)
                messageContent = SerializeBulkStrings(charArrayValue);
            else if (messageObject is Exception exceptionValue)
                messageContent = SerializeErrorType(exceptionValue);
            else if (messageObject is int intValue)
                messageContent = SerializeIntegerType(intValue);
            else if (messageObject is long longValue)
                messageContent= SerializeLongType(longValue);
            else if (messageObject is ICollection collectionValue)
                messageContent = SerializeArrayType(collectionValue);

            return new RESPMessage(messageContent, false);
        }
        catch (Exception ex)
        {
            return new RESPMessage(SerializeErrorType(ex), false);
        }
    }

    private string SerializeSimpleStringType(string stringType)
    {
        return $"{RESPConstants.SimpleStringType}{stringType}{RESPConstants.Terminator}";
    }

    private string SerializeErrorType(Exception exception)
    {
        return $"{RESPConstants.ErrorType}{exception.Message}{RESPConstants.Terminator}";
    }

    private string SerializeIntegerType(int integerType)
    {
        return $"{RESPConstants.IntegerType}{integerType}{RESPConstants.Terminator}";
    }

    private string SerializeLongType(long longType)
    {
        return $"{RESPConstants.IntegerType}{longType}{RESPConstants.Terminator}";
    }

    private string SerializeNull()
    {
        return RESPConstants.NullValues[0];
    }

    private string SerializeBulkStrings(char[] bulkString)
    {
        return $"{RESPConstants.BulkStringType}{bulkString.Length}{RESPConstants.Terminator}{new string(bulkString)}{RESPConstants.Terminator}";
    }

    private string SerializeArrayType(ICollection data)
    {
        if (data.Count == 0)
        {
            return RESPConstants.EmptyArray;
        }

        var stringBuilder = new StringBuilder();
        stringBuilder.Append(RESPConstants.ArrayType);
        stringBuilder.Append(data.Count);
        stringBuilder.Append(RESPConstants.Terminator);

        foreach (var item in data)
        {
            if (item is null)
                stringBuilder.Append(SerializeNull());
            else if (item is int intItem)
                stringBuilder.Append(SerializeIntegerType(intItem));
            else if (item is string stringItem)
                stringBuilder.Append(SerializeBulkStrings(stringItem.ToCharArray()));
            else if (item is char[] charItem)
                stringBuilder.Append(SerializeBulkStrings(charItem));
            else if (item is Exception exceptionItem)
                stringBuilder.Append(SerializeErrorType(exceptionItem));
            else if (item is ICollection collectionItem)
                stringBuilder.Append(SerializeArrayType(collectionItem));
        }

        return stringBuilder.ToString();
    }
}
