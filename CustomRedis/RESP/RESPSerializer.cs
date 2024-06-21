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
            else if (messageObject is string)
                messageContent = SerializeSimpleStringType((string)messageObject);
            else if (messageObject is char[])
                messageContent = SerializeBulkStrings((char[])messageObject);
            else if (messageObject is Exception)
                messageContent = SerializeErrorType((Exception)messageObject);
            else if (messageObject is int)
                messageContent = SerializeIntegerType((int)messageObject);
            else if (messageObject is long)
                messageContent= SerializeLongType((long)messageObject);
            else if (messageObject is ICollection<object>)
                messageContent = SerializeArrayType((ICollection<object>)messageObject);

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

    private string SerializeArrayType(ICollection<object> data)
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
            else if (item is int)
                stringBuilder.Append(SerializeIntegerType((int)item));
            else if (item is string)
                stringBuilder.Append(SerializeSimpleStringType((string)item));
            else if (item is char[])
                stringBuilder.Append(SerializeBulkStrings((char[])item));
            else if (item is Exception)
                stringBuilder.Append(SerializeErrorType((Exception)item));
            else if (item is ICollection<object>)
                stringBuilder.Append(SerializeArrayType((ICollection<object>)item));
        }

        return stringBuilder.ToString();
    }
}
