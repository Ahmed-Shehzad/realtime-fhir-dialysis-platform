using BuildingBlocks;

using FinancialInteroperability.Domain.Events;

using RealtimePlatform.IntegrationEventCatalog;

namespace FinancialInteroperability.Domain;

/// <summary>Input for <see cref="PatientCoverageRegistration.Register"/> (keeps factory arity within analyzer limits).</summary>
public sealed record PatientCoverageRegistrationRegisterPayload
{
    public required string PatientId { get; init; }

    public required string MemberIdentifier { get; init; }

    public required string PayorDisplayName { get; init; }

    public required string PlanDisplayName { get; init; }

    public required DateOnly PeriodStart { get; init; }

    public DateOnly? PeriodEnd { get; init; }

    public string? FhirCoverageResourceId { get; init; }

    public string? TenantId { get; init; }
}

public sealed class PatientCoverageRegistration : AggregateRoot
{
    public const int MaxPatientIdLength = 256;

    public const int MaxMemberIdentifierLength = 128;

    public const int MaxPayorDisplayNameLength = 512;

    public const int MaxPlanDisplayNameLength = 512;

    public const int MaxFhirCoverageIdLength = 128;

    public const int MaxTenantIdLength = 128;

    private PatientCoverageRegistration()
    {
    }

    public string PatientId { get; private set; } = null!;

    public string MemberIdentifier { get; private set; } = null!;

    public string PayorDisplayName { get; private set; } = null!;

    public string PlanDisplayName { get; private set; } = null!;

    public DateOnly PeriodStart { get; private set; }

    public DateOnly? PeriodEnd { get; private set; }

    public string? FhirCoverageResourceId { get; private set; }

    public string? TenantId { get; private set; }

    public static PatientCoverageRegistration Register(Ulid correlationId, PatientCoverageRegistrationRegisterPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        string pid = ValidateAndTrim(
            payload.PatientId,
            nameof(PatientCoverageRegistrationRegisterPayload.PatientId),
            MaxPatientIdLength);
        string member = ValidateAndTrim(
            payload.MemberIdentifier,
            nameof(PatientCoverageRegistrationRegisterPayload.MemberIdentifier),
            MaxMemberIdentifierLength);
        string payor = ValidateAndTrim(
            payload.PayorDisplayName,
            nameof(PatientCoverageRegistrationRegisterPayload.PayorDisplayName),
            MaxPayorDisplayNameLength);
        string plan = ValidateAndTrim(
            payload.PlanDisplayName,
            nameof(PatientCoverageRegistrationRegisterPayload.PlanDisplayName),
            MaxPlanDisplayNameLength);
        string? fhirId = NormalizeOptional(payload.FhirCoverageResourceId, MaxFhirCoverageIdLength);
        string? tenant = NormalizeOptional(payload.TenantId, MaxTenantIdLength);

        var registration = new PatientCoverageRegistration
        {
            PatientId = pid,
            MemberIdentifier = member,
            PayorDisplayName = payor,
            PlanDisplayName = plan,
            PeriodStart = payload.PeriodStart,
            PeriodEnd = payload.PeriodEnd,
            FhirCoverageResourceId = fhirId,
            TenantId = tenant,
        };
        registration.ApplyCreatedDateTime();
        registration.ApplyEvent(new PatientCoverageRegisteredDomainEvent(registration.Id, pid));
        registration.ApplyEvent(
            new PatientCoverageSnapshotRecordedIntegrationEvent(correlationId, registration.Id.ToString())
            {
                TenantId = tenant,
                PatientId = pid,
            });
        return registration;
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
