namespace FinancialInteroperability.Domain.Abstractions;

public sealed record PatientCoverageRegistrationSummary(
    Ulid Id,
    string PatientId,
    string MemberIdentifier,
    string PayorDisplayName,
    string PlanDisplayName,
    DateOnly PeriodStart,
    DateOnly? PeriodEnd,
    string? FhirCoverageResourceId,
    DateTime CreatedAtUtc);

public sealed record CoverageEligibilityInquirySummary(
    Ulid Id,
    Ulid PatientCoverageRegistrationId,
    string PatientId,
    EligibilityInquiryStatus Status,
    string OutcomeCode,
    string? Notes,
    DateTime CreatedAtUtc);

public sealed record DialysisFinancialClaimSummary(
    Ulid Id,
    string TreatmentSessionId,
    string PatientId,
    Ulid PatientCoverageRegistrationId,
    string? FhirEncounterReference,
    FinancialClaimUse ClaimUse,
    FinancialClaimLifecycleStatus Status,
    string? ExternalClaimId,
    string? ExternalClaimResponseId,
    string? OutcomeDisplay,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record ExplanationOfBenefitSummary(
    Ulid Id,
    Ulid DialysisFinancialClaimId,
    string TreatmentSessionId,
    string FhirExplanationOfBenefitReference,
    decimal? PatientResponsibilityAmount,
    DateTimeOffset LinkedAtUtc,
    DateTime CreatedAtUtc);

public sealed record FinancialSessionTimelineReadModel(
    string TreatmentSessionId,
    string? ResolvedPatientId,
    IReadOnlyList<PatientCoverageRegistrationSummary> Coverages,
    IReadOnlyList<CoverageEligibilityInquirySummary> EligibilityInquiries,
    IReadOnlyList<DialysisFinancialClaimSummary> Claims,
    IReadOnlyList<ExplanationOfBenefitSummary> ExplanationOfBenefits);
