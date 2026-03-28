using BuildingBlocks;

using FinancialInteroperability.Domain.Events;

using RealtimePlatform.IntegrationEventCatalog;

namespace FinancialInteroperability.Domain;

public sealed class CoverageEligibilityInquiry : AggregateRoot
{
    public const int MaxOutcomeCodeLength = 64;

    public const int MaxNotesLength = 2000;

    public const int MaxTenantIdLength = 128;

    private CoverageEligibilityInquiry()
    {
    }

    public Ulid PatientCoverageRegistrationId { get; private set; }

    public string PatientId { get; private set; } = null!;

    public EligibilityInquiryStatus Status { get; private set; }

    public string OutcomeCode { get; private set; } = null!;

    public string? Notes { get; private set; }

    public string? TenantId { get; private set; }

    public static CoverageEligibilityInquiry Complete(
        Ulid correlationId,
        Ulid patientCoverageRegistrationId,
        string patientId,
        string outcomeCode,
        string? notes,
        string? tenantId)
    {
        if (patientCoverageRegistrationId == Ulid.Empty)
            throw new ArgumentException("Registration id is required.", nameof(patientCoverageRegistrationId));

        ArgumentException.ThrowIfNullOrWhiteSpace(patientId);
        string pid = patientId.Trim();
        if (pid.Length > PatientCoverageRegistration.MaxPatientIdLength)
            throw new ArgumentException("PatientId exceeds max length.", nameof(patientId));

        ArgumentException.ThrowIfNullOrWhiteSpace(outcomeCode);
        string outcome = outcomeCode.Trim();
        if (outcome.Length > MaxOutcomeCodeLength)
            throw new ArgumentException("OutcomeCode exceeds max length.", nameof(outcomeCode));

        string? n = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        if (n is not null && n.Length > MaxNotesLength)
            n = n[..MaxNotesLength];

        string? tenant = string.IsNullOrWhiteSpace(tenantId) ? null : tenantId.Trim();
        if (tenant is not null && tenant.Length > MaxTenantIdLength)
            tenant = tenant[..MaxTenantIdLength];

        var inquiry = new CoverageEligibilityInquiry
        {
            PatientCoverageRegistrationId = patientCoverageRegistrationId,
            PatientId = pid,
            Status = EligibilityInquiryStatus.Completed,
            OutcomeCode = outcome,
            Notes = n,
            TenantId = tenant,
        };
        inquiry.ApplyCreatedDateTime();
        inquiry.ApplyEvent(new CoverageEligibilityRecordedDomainEvent(inquiry.Id, pid, outcome));
        inquiry.ApplyEvent(
            new CoverageEligibilityOutcomeRecordedIntegrationEvent(correlationId, inquiry.Id.ToString(), outcome)
            {
                TenantId = tenant,
                PatientId = pid,
            });
        return inquiry;
    }
}
