using BuildingBlocks;

namespace FinancialInteroperability.Domain.Events;

public sealed record ExplanationOfBenefitAttachedDomainEvent(
    Ulid ExplanationOfBenefitId,
    string TreatmentSessionId) : DomainEvent;
