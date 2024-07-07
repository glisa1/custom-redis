namespace RESP;

public class RESPParser
{
    private readonly RESPDeserializer _deserializer;
    private readonly RESPSerializer _serializer;

    public RESPParser()
    {
        _deserializer = new RESPDeserializer();
        _serializer = new RESPSerializer();
    }

    public List<string> DeserializeMessage(string message)
    {
        return _deserializer.DeserializeMessage(message);
    }

    public RESPMessage SerializeMessage(object? messageObject)
    {
        return _serializer.SerializeMessage(messageObject);
    }
}
