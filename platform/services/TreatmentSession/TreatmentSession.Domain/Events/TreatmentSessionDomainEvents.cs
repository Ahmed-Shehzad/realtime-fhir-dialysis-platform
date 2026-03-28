using BuildingBlocks;

namespace TreatmentSession.Domain.Events;

public sealed record DialysisSessionCreatedDomainEvent(Ulid SessionAggregateId) : DomainEvent;

public sealed record PatientAssignedToSessionDomainEvent(Ulid SessionAggregateId, string MedicalRecordNumber) : DomainEvent;

public sealed record DialysisSessionStartedDomainEvent(Ulid SessionAggregateId) : DomainEvent;

public sealed record DialysisSessionCompletedDomainEvent(Ulid SessionAggregateId) : DomainEvent;

public sealed record MeasurementContextResolvedDomainEvent(Ulid SessionAggregateId, string MeasurementId) : DomainEvent;

public sealed record MeasurementContextUnresolvedDomainEvent(Ulid SessionAggregateId, string MeasurementId, string Reason) : DomainEvent;
