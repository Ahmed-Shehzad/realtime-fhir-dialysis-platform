using BuildingBlocks.Abstractions;
using BuildingBlocks.IntegrationEvents;

using RealtimePlatform.MassTransit;

namespace BuildingBlocks.Persistence;

/// <summary>Maps domain <see cref="IIntegrationEvent"/> instances to <see cref="CatalogIntegrationEventTransport"/> for MassTransit.</summary>
public static class IntegrationEventCatalogTransportMapper
{
    /// <summary>Builds a broker message compatible with the integration event catalog §1 envelope in <see cref="CatalogIntegrationEventTransport.Payload"/>.</summary>
    public static CatalogIntegrationEventTransport ToTransport(
        IIntegrationEvent integrationEvent,
        RealtimePlatform.Messaging.IMessageSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(integrationEvent);
        ArgumentNullException.ThrowIfNull(serializer);

        Type messageType = integrationEvent.GetType();
        Ulid messageId = integrationEvent is IntegrationEvent integration
            ? integration.EventId
            : Ulid.NewUlid();

        ReadOnlyMemory<byte> body = integrationEvent is IntegrationEvent envelopeEvent
            ? IntegrationEventTransportSerializer.SerializeToUtf8Bytes(envelopeEvent)
            : serializer.Serialize(integrationEvent, messageType);

        return new CatalogIntegrationEventTransport
        {
            MessageId = messageId.ToString(),
            CorrelationId = integrationEvent.CorrelationId?.ToString(),
            MessageType = messageType.FullName ?? messageType.Name,
            ContentType = serializer.ContentType,
            Payload = body.ToArray(),
        };
    }
}
