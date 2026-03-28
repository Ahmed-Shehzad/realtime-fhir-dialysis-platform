using FinancialInteroperability.Domain;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Commands.SubmitDialysisFinancialClaim;

public sealed record SubmitDialysisFinancialClaimCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string PatientId,
    Ulid PatientCoverageRegistrationId,
    string? FhirEncounterReference,
    FinancialClaimUse ClaimUse,
    string? ExternalClaimId,
    string? AuthenticatedUserId = null) : ICommand<SubmitDialysisFinancialClaimResult>;
