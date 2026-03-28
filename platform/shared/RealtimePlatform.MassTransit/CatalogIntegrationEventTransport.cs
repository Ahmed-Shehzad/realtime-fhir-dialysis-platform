namespace RealtimePlatform.MassTransit;

/// <summary>
/// Broker message wrapping the §1 catalog JSON envelope bytes staged by MassTransit EF transactional outbox (<see cref="Payload"/>).
/// </summary>
public sealed record CatalogIntegrationEventTransport
{
    /// <summary>Catalog <c>eventId</c> / logical message id.</summary>
    public required string MessageId { get; init; }

    /// <summary>Optional correlation id for distributed tracing.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Logical .NET message type from outbox.</summary>
    public required string MessageType { get; init; }

    /// <summary>Payload content type (e.g. application/json).</summary>
    public required string ContentType { get; init; }

    /// <summary>UTF-8 §1 envelope JSON (or transport-specific bytes).</summary>
    public required byte[] Payload { get; init; }
}
