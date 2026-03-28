using BuildingBlocks;

using BuildingBlocks.ValueObjects;

namespace MeasurementAcquisition.Domain.Events;

public sealed record MeasurementReceivedDomainEvent(Ulid MeasurementId, DeviceId DeviceId) : DomainEvent;

public sealed record MeasurementAcceptedDomainEvent(Ulid MeasurementId) : DomainEvent;

public sealed record MeasurementRejectedDomainEvent(Ulid MeasurementId, string Reason) : DomainEvent;
