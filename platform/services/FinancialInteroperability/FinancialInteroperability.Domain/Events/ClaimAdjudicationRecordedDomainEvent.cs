using BuildingBlocks;

namespace FinancialInteroperability.Domain.Events;

public sealed record ClaimAdjudicationRecordedDomainEvent(
    Ulid FinancialClaimId,
    string ExternalClaimResponseId) : DomainEvent;
