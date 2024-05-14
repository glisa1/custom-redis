using System;
using System.Text;

namespace RESP;

public class RESPDeserializer
{
    public List<string> DeserializeMessage(string message)
    {
        var respMessage = new RESPMessage(message, true);

        return DeserializeArrayType(respMessage.Message);
    }

    private List<string> DeserializeArrayType(string message)
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

    private List<string> GetCommands(string messagePart, int numberOfArrayElements)
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

    private string RemoveOperation(string message)
    {
        return message[1..];
    }

    private bool IsNumberOfParametersInMessageCorrect(string messagePart, int numberOfExpectedParameters)
    {
        var messageParts = messagePart.Split(RESPConstants.Terminator);

        return messageParts.Length - 1 == numberOfExpectedParameters * 2;
    }
}
