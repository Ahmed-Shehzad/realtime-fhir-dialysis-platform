using BuildingBlocks;

namespace RealtimePlatform.IntegrationEventCatalog;

/// <summary>Published when a measurement-related outbox message is quarantined after repeated transport failures (catalog — Measurement Acquisition).</summary>
public sealed record MeasurementQuarantinedIntegrationEvent(
    Ulid CorrelationId,
    string OriginalOutboxMessageId,
    string OriginalMessageType,
    string PublishFailureSummary) : IntegrationEvent(CorrelationId);
