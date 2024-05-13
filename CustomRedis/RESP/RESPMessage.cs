namespace RESP;

public class RESPMessage
{
    public string Message { get; }

    private bool Incoming { get; }

    public RESPMessage(string message, bool incoming)
    {
        Message = message;
        Incoming = incoming;

        ValidateMessage();
    }

    private void ValidateMessage()
    {
        if (string.IsNullOrWhiteSpace(Message))
            throw new ArgumentException(nameof(Message));

        if (Incoming && Message[0] != RESPConstants.ArrayType)
            throw new ArgumentException("Message in wrong format. Expected Array type.");
    }
}
