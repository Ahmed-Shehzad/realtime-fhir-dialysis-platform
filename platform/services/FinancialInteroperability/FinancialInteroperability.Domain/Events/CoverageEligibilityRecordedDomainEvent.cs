using BuildingBlocks;

namespace FinancialInteroperability.Domain.Events;

public sealed record CoverageEligibilityRecordedDomainEvent(
    Ulid EligibilityInquiryId,
    string PatientId,
    string OutcomeCode) : DomainEvent;
