using BuildingBlocks;

namespace FinancialInteroperability.Domain.Events;

public sealed record DialysisFinancialClaimSubmittedDomainEvent(
    Ulid FinancialClaimId,
    string TreatmentSessionId) : DomainEvent;
