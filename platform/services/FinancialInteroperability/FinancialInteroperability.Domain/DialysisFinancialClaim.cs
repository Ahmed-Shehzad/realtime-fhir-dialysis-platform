using BuildingBlocks;

using FinancialInteroperability.Domain.Events;

using RealtimePlatform.IntegrationEventCatalog;

namespace FinancialInteroperability.Domain;

/// <summary>Input for <see cref="DialysisFinancialClaim.Submit"/> (keeps factory arity within analyzer limits).</summary>
public sealed record DialysisFinancialClaimSubmitPayload
{
    public required string TreatmentSessionId { get; init; }

    public required string PatientId { get; init; }

    public required Ulid PatientCoverageRegistrationId { get; init; }

    public string? FhirEncounterReference { get; init; }

    public required FinancialClaimUse ClaimUse { get; init; }

    public string? ExternalClaimId { get; init; }

    public string? TenantId { get; init; }
}

public sealed class DialysisFinancialClaim : AggregateRoot
{
    public const int MaxTreatmentSessionIdLength = 64;

    public const int MaxPatientIdLength = 256;

    public const int MaxFhirEncounterReferenceLength = 2048;

    public const int MaxExternalIdLength = 256;

    public const int MaxOutcomeDisplayLength = 1024;

    public const int MaxTenantIdLength = 128;

    private DialysisFinancialClaim()
    {
    }

    public string TreatmentSessionId { get; private set; } = null!;

    public string PatientId { get; private set; } = null!;

    public Ulid PatientCoverageRegistrationId { get; private set; }

    public string? FhirEncounterReference { get; private set; }

    public FinancialClaimUse ClaimUse { get; private set; }

    public FinancialClaimLifecycleStatus Status { get; private set; }

    public string? ExternalClaimId { get; private set; }

    public string? ExternalClaimResponseId { get; private set; }

    public string? OutcomeDisplay { get; private set; }

    public string? TenantId { get; private set; }

    public static DialysisFinancialClaim Submit(Ulid correlationId, DialysisFinancialClaimSubmitPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        string sessionId = ValidateAndTrim(
            payload.TreatmentSessionId,
            nameof(DialysisFinancialClaimSubmitPayload.TreatmentSessionId),
            MaxTreatmentSessionIdLength);
        string pid = ValidateAndTrim(
            payload.PatientId,
            nameof(DialysisFinancialClaimSubmitPayload.PatientId),
            MaxPatientIdLength);
        if (payload.PatientCoverageRegistrationId == Ulid.Empty)
            throw new ArgumentException("Coverage registration is required.", nameof(payload));

        string? encounter = NormalizeOptional(payload.FhirEncounterReference, MaxFhirEncounterReferenceLength);
        string? extClaim = NormalizeOptional(payload.ExternalClaimId, MaxExternalIdLength);
        string? tenant = NormalizeOptional(payload.TenantId, MaxTenantIdLength);

        var claim = new DialysisFinancialClaim
        {
            TreatmentSessionId = sessionId,
            PatientId = pid,
            PatientCoverageRegistrationId = payload.PatientCoverageRegistrationId,
            FhirEncounterReference = encounter,
            ClaimUse = payload.ClaimUse,
            Status = FinancialClaimLifecycleStatus.Submitted,
            ExternalClaimId = extClaim,
            TenantId = tenant,
        };
        claim.ApplyCreatedDateTime();
        claim.ApplyEvent(new DialysisFinancialClaimSubmittedDomainEvent(claim.Id, sessionId));
        claim.ApplyEvent(
            new DialysisFinancialClaimSubmittedIntegrationEvent(correlationId, claim.Id.ToString(), sessionId)
            {
                TenantId = tenant,
                PatientId = pid,
                SessionId = sessionId,
            });
        return claim;
    }

    public void RecordAdjudication(Ulid correlationId, string externalClaimResponseId, string? outcomeDisplay)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(externalClaimResponseId);
        string responseId = externalClaimResponseId.Trim();
        if (responseId.Length > MaxExternalIdLength)
            throw new ArgumentException("External claim response id exceeds max length.", nameof(externalClaimResponseId));

        if (Status == FinancialClaimLifecycleStatus.Adjudicated)
        {
            if (ExternalClaimResponseId is not null
                && string.Equals(ExternalClaimResponseId, responseId, StringComparison.Ordinal))
                return;
            throw new InvalidOperationException("Claim is already adjudicated with a different response.");
        }

        if (Status != FinancialClaimLifecycleStatus.Submitted)
            throw new InvalidOperationException("Adjudication requires a submitted claim.");

        ExternalClaimResponseId = responseId;
        OutcomeDisplay = NormalizeOptional(outcomeDisplay, MaxOutcomeDisplayLength);
        Status = FinancialClaimLifecycleStatus.Adjudicated;
        ApplyUpdateDateTime();
        ApplyEvent(new ClaimAdjudicationRecordedDomainEvent(Id, responseId));
        ApplyEvent(
            new ClaimAdjudicationRecordedIntegrationEvent(correlationId, Id.ToString(), responseId)
            {
                TenantId = TenantId,
                PatientId = PatientId,
                SessionId = TreatmentSessionId,
            });
    }

    private static string ValidateAndTrim(string value, string fieldName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        string t = value.Trim();
        if (t.Length > maxLength)
            throw new ArgumentException($"{fieldName} exceeds max length.");
        return t;
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string t = value.Trim();
        return t.Length > maxLength ? t[..maxLength] : t;
    }
}
