namespace RESP;

public class RESPMessage
{
    public string Message { get; }

    private bool Incoming { get; }

    public RESPMessage(string message, bool incoming)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException(nameof(message));

        if (incoming && message[0] != RESPConstants.ArrayType)
            throw new ArgumentException("Message in wrong format. Expected Array type.");

        Message = message;
        Incoming = incoming;
    }
}
