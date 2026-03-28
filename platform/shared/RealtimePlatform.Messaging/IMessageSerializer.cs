namespace RealtimePlatform.Messaging;

/// <summary>
/// Serializes integration messages for outbox persistence and transport.
/// </summary>
public interface IMessageSerializer
{
    /// <summary> MIME or format identifier for the serialized body (for example application/json). </summary>
    string ContentType { get; }

    ReadOnlyMemory<byte> Serialize(object message, Type runtimeType);
}
