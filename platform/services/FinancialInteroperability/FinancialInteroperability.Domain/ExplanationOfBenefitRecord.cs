using BuildingBlocks;

using FinancialInteroperability.Domain.Events;

using RealtimePlatform.IntegrationEventCatalog;

namespace FinancialInteroperability.Domain;

public sealed class ExplanationOfBenefitRecord : AggregateRoot
{
    public const int MaxTreatmentSessionIdLength = 64;

    public const int MaxFhirEobReferenceLength = 2048;

    public const int MaxTenantIdLength = 128;

    /// <summary>Scale: cents or smallest currency unit; nullable when unknown.</summary>
    public decimal? PatientResponsibilityAmount { get; private set; }

    private ExplanationOfBenefitRecord()
    {
    }

    public Ulid DialysisFinancialClaimId { get; private set; }

    public string TreatmentSessionId { get; private set; } = null!;

    public string FhirExplanationOfBenefitReference { get; private set; } = null!;

    public DateTimeOffset LinkedAtUtc { get; private set; }

    public string? TenantId { get; private set; }

    public static ExplanationOfBenefitRecord Attach(
        Ulid correlationId,
        Ulid dialysisFinancialClaimId,
        string treatmentSessionId,
        string fhirExplanationOfBenefitReference,
        decimal? patientResponsibilityAmount,
        string? tenantId)
    {
        if (dialysisFinancialClaimId == Ulid.Empty)
            throw new ArgumentException("Claim id is required.", nameof(dialysisFinancialClaimId));

        ArgumentException.ThrowIfNullOrWhiteSpace(treatmentSessionId);
        string sid = treatmentSessionId.Trim();
        if (sid.Length > MaxTreatmentSessionIdLength)
            throw new ArgumentException("TreatmentSessionId exceeds max length.", nameof(treatmentSessionId));

        ArgumentException.ThrowIfNullOrWhiteSpace(fhirExplanationOfBenefitReference);
        string eobRef = fhirExplanationOfBenefitReference.Trim();
        if (eobRef.Length > MaxFhirEobReferenceLength)
            throw new ArgumentException("EOB reference exceeds max length.", nameof(fhirExplanationOfBenefitReference));

        string? tenant = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
        if (tenant is not null && tenant.Length > MaxTenantIdLength)
            tenant = tenant[..MaxTenantIdLength];

        var row = new ExplanationOfBenefitRecord
        {
            DialysisFinancialClaimId = dialysisFinancialClaimId,
            TreatmentSessionId = sid,
            FhirExplanationOfBenefitReference = eobRef,
            PatientResponsibilityAmount = patientResponsibilityAmount,
            LinkedAtUtc = DateTimeOffset.UtcNow,
            TenantId = tenant,
        };
        row.ApplyCreatedDateTime();
        row.ApplyEvent(new ExplanationOfBenefitAttachedDomainEvent(row.Id, sid));
        row.ApplyEvent(
            new ExplanationOfBenefitLinkedToSessionIntegrationEvent(correlationId, row.Id.ToString(), sid)
            {
                TenantId = tenant,
                SessionId = sid,
            });
        return row;
    }
}
