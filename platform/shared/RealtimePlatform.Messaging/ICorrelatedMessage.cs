namespace RealtimePlatform.Messaging;

/// <summary>
/// Carries a correlation identifier for distributed tracing across handlers and brokers.
/// </summary>
public interface ICorrelatedMessage
{
    Ulid? CorrelationId { get; }
}
