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
        if (messageObject == null)
            throw new ArgumentNullException("message");

        var messageContent = string.Empty;
        if (messageObject is string)
            messageContent = SerializeSimpleStringType((string)messageObject);
        else if (messageObject is Exception)
            messageContent = SerializeErrorType((Exception)messageObject);

        return new RESPMessage(messageContent, false);
    }

    //private void DeserializeIntegerType()
    //{
    //    var messageData = RemoveOperation();

    //    int deserializedValue = int.Parse(messageData);
    //}

    //private void DeserializeBulkStringType()
    //{
    //    var messageData = RemoveOperation();

    //    var dataLengthAndData = messageData.Split(RESPConstants.Terminator);

    //    if (dataLengthAndData.Length != 2)
    //        throw new ArgumentException();

    //    var dataLength = int.Parse(dataLengthAndData[0]);
    //    var data = dataLengthAndData[1];
    //}

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

    private static string SerializeSimpleStringType(string message)
    {
        return $"{RESPConstants.SimpleStringType}{message}{RESPConstants.Terminator}";
    }

    private static string SerializeErrorType(Exception exception)
    {
        return $"{RESPConstants.ErrorType}{exception.Message}{RESPConstants.Terminator}";
    }
}
