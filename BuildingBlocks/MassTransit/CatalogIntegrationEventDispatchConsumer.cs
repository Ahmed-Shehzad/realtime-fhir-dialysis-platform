using BuildingBlocks.IntegrationEvents;

using Intercessor.Abstractions;

using MassTransit;

using Microsoft.Extensions.Logging;

using RealtimePlatform.MassTransit;

namespace BuildingBlocks.MassTransit;

/// <summary>
/// Consumes <see cref="CatalogIntegrationEventTransport"/> from the integration-events topic (or in-memory fan-out), maps the §1 envelope to <see cref="IntegrationEvent"/>, and publishes in-process for <see cref="INotificationHandler{TNotification}"/> dispatch.
/// MassTransit EF inbox provides idempotent delivery for the same broker-assigned message id.
/// </summary>
public sealed class CatalogIntegrationEventDispatchConsumer : IConsumer<CatalogIntegrationEventTransport>
{
    private readonly IPublisher _publisher;
    private readonly ILogger<CatalogIntegrationEventDispatchConsumer> _logger;

    public CatalogIntegrationEventDispatchConsumer(
        IPublisher publisher,
        ILogger<CatalogIntegrationEventDispatchConsumer> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CatalogIntegrationEventTransport> context)
    {
        CatalogIntegrationEventTransport message = context.Message;
        if (message.Payload is not { Length: > 0 })
        {
            _logger.LogWarning(
                "Catalog integration event transport missing payload. MessageId={MessageId}",
                message.MessageId);
            throw new InvalidOperationException("Integration event transport payload is empty.");
        }

        global::BuildingBlocks.IntegrationEvent integrationEvent;
        try
        {
            integrationEvent = IntegrationEventTransportSerializer.DeserializeIntegrationEventFromCatalogEnvelope(
                message.Payload);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "Failed to deserialize catalog integration event. MessageId={MessageId} MessageType={MessageType}",
                message.MessageId,
                message.MessageType);
            throw new InvalidOperationException(
                $"Failed to deserialize catalog integration event. MessageId={message.MessageId} MessageType={message.MessageType}",
                ex);
        }

        await _publisher.PublishAsync((dynamic)integrationEvent, context.CancellationToken).ConfigureAwait(false);
    }
}
