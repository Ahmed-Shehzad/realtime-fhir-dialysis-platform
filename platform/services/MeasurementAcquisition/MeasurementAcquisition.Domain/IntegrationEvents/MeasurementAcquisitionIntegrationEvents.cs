using BuildingBlocks;

namespace MeasurementAcquisition.Domain.IntegrationEvents;

public sealed record MeasurementReceivedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string DeviceId,
    string Channel,
    string MeasurementType) : IntegrationEvent(CorrelationId);

public sealed record MeasurementRejectedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string DeviceId,
    string Reason) : IntegrationEvent(CorrelationId);
