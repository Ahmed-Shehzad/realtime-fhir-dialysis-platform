using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.AttachExplanationOfBenefit;

public sealed record AttachExplanationOfBenefitCommand(
    Ulid CorrelationId,
    Ulid DialysisFinancialClaimId,
    string TreatmentSessionId,
    string FhirExplanationOfBenefitReference,
    decimal? PatientResponsibilityAmount,
    string? AuthenticatedUserId = null) : ICommand<AttachExplanationOfBenefitResult>;
