using System;
using System.Text;

namespace RESP;

public static class RESPDeserializer
{
    public static List<string> DeserializeMessage(string message)
    {
        var respMessage = new RESPMessage(message, true);

        return DeserializeArrayType(respMessage.Message);
    }

    public static RESPMessage SerializeMessage(object messageObject)
    {
        var messageContent = string.Empty;
        if (messageObject == null)
            messageContent = SerializeNull();
        else if (messageObject is string)
            messageContent = SerializeSimpleStringType((string)messageObject);
        else if (messageObject is Exception)
            messageContent = SerializeErrorType((Exception)messageObject);
        else if (messageObject is int)
            messageContent = SerializeIntegerType((int)messageObject);
        else if (messageObject is ICollection<object>)
            messageContent = SerializeArrayType((ICollection<object>)messageObject);

            return new RESPMessage(messageContent, false);
    }

    public static RESPMessage SerializeBulkString(string message)
    {
        return new RESPMessage(SerializeBulkStrings(message), false);
    }

    private static List<string> DeserializeArrayType(string message)
    {
        var messageData = RemoveOperation(message);

        var indexOfFirstTerminator = messageData.IndexOf(RESPConstants.Terminator);

        var numberOfArrayElements = int.Parse(messageData.Substring(0, indexOfFirstTerminator));

        if (numberOfArrayElements < 1)
            throw new ArgumentException("Message in wrong format. Unexpected number of array elements.");

        var arrayElementsMessagePart = messageData.Substring(indexOfFirstTerminator + RESPConstants.Terminator.Length);

        if (!IsNumberOfParametersInMessageCorrect(arrayElementsMessagePart, numberOfArrayElements))
            throw new ArgumentException("Message in wrong format. Unexpected number of array elements.");

        return GetCommands(arrayElementsMessagePart, numberOfArrayElements);
    }

    private static List<string> GetCommands(string messagePart, int numberOfArrayElements)
    {
        var commands = new List<string>();
        var messageToWorkOn = messagePart;
        for (var i = 0; i < numberOfArrayElements; i++)
        {
            if (messageToWorkOn[0] != RESPConstants.BulkStringType)
                throw new ArgumentException("Message in wrong format. Expected BulkString type.");

            messageToWorkOn = RemoveOperation(messageToWorkOn);
            var indexOfFirstTerminator = messageToWorkOn.IndexOf(RESPConstants.Terminator);
            if (indexOfFirstTerminator == -1)
                throw new ArgumentException("Message in wrong format.");

            var stringLength = int.Parse(messageToWorkOn.Substring(0, indexOfFirstTerminator));
            var command = messageToWorkOn.Substring(indexOfFirstTerminator + RESPConstants.Terminator.Length, stringLength);
            if (string.IsNullOrEmpty(command))
                throw new ArgumentException("Message in wrong format.");

            commands.Add(command);

            messageToWorkOn = messageToWorkOn.Substring(indexOfFirstTerminator + RESPConstants.Terminator.Length * 2 + stringLength);
        }

        return commands;
    }

    private static string RemoveOperation(string message)
    {
        return message[1..];
    }

    private static bool IsNumberOfParametersInMessageCorrect(string messagePart, int numberOfExpectedParameters)
    {
        var messageParts = messagePart.Split(RESPConstants.Terminator);

        return messageParts.Length - 1 == numberOfExpectedParameters * 2;
    }

    private static string SerializeSimpleStringType(string stringType)
    {
        return $"{RESPConstants.SimpleStringType}{stringType}{RESPConstants.Terminator}";
    }

    private static string SerializeErrorType(Exception exception)
    {
        return $"{RESPConstants.ErrorType}{exception.Message}{RESPConstants.Terminator}";
    }

    private static string SerializeIntegerType(int integerType)
    {
        return $"{RESPConstants.IntegerType}{integerType}{RESPConstants.Terminator}";
    }

    private static string SerializeNull()
    {
        return RESPConstants.NullValues[0];
    }

    private static string SerializeBulkStrings(string bulkString)
    {
        return $"{RESPConstants.BulkStringType}{bulkString.Length}{RESPConstants.Terminator}{bulkString}{RESPConstants.Terminator}";
    }

    private static string SerializeArrayType(ICollection<object> data)
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
                stringBuilder.Append(SerializeBulkStrings((string)item));
            else if (item is ICollection<object>)
                stringBuilder.Append(SerializeArrayType((ICollection<object>)item));
        }

        return stringBuilder.ToString();
    }
}
