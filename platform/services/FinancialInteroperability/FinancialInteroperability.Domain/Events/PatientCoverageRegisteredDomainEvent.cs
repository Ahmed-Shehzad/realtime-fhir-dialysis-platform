using BuildingBlocks;

namespace FinancialInteroperability.Domain.Events;

public sealed record PatientCoverageRegisteredDomainEvent(
    Ulid CoverageRegistrationId,
    string PatientId) : DomainEvent;
