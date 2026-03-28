using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace TerminologyConformance.Domain;

/// <summary>MVP terminology + FHIR profile URL conformance check for a logical resource identifier.</summary>
public sealed class ConformanceAssessment : AggregateRoot
{
    public const int MaxResourceIdLength = 256;

    public const int MaxReasonLength = 2000;

    public const int MaxProfileUrlLength = 2048;

    public const int MaxCodeSystemUriLength = 512;

    public const int MaxCodeValueLength = 512;

    public const int MaxUnitCodeLength = 64;

    public const string DefaultProfileRuleRegistryVersion = "profile-rules-v1";

    private ConformanceAssessment()
    {
    }

    public string ResourceId { get; private set; } = null!;

    public ConformanceSliceOutcome TerminologySliceOutcome { get; private set; }

    public ConformanceSliceOutcome ProfileSliceOutcome { get; private set; }

    public string? TerminologyReason { get; private set; }

    public string? ProfileReason { get; private set; }

    public string? AssessedProfileUrl { get; private set; }

    public string ProfileRuleRegistryVersion { get; private set; } = null!;

    public DateTimeOffset EvaluatedAtUtc { get; private set; }

    public static ConformanceAssessment Run(
        Ulid correlationId,
        string resourceId,
        string? codeSystemUri,
        string? codeValue,
        string? unitCode,
        string? profileUrl,
        string? tenantId)
    {
        string rid = (resourceId ?? string.Empty).Trim();
        if (rid.Length == 0 || rid.Length > MaxResourceIdLength)
            throw new ArgumentException("ResourceId is invalid.", nameof(resourceId));

        string? cs = NormalizeOptional(codeSystemUri, MaxCodeSystemUriLength);
        string? cv = NormalizeOptional(codeValue, MaxCodeValueLength);
        string? uc = NormalizeOptional(unitCode, MaxUnitCodeLength);
        string? pu = NormalizeOptional(profileUrl, MaxProfileUrlLength);

        bool terminologyActive = !string.IsNullOrEmpty(cs) || !string.IsNullOrEmpty(cv) || !string.IsNullOrEmpty(uc);
        bool profileActive = !string.IsNullOrEmpty(pu);

        var assessment = new ConformanceAssessment
        {
            ResourceId = rid,
            ProfileRuleRegistryVersion = DefaultProfileRuleRegistryVersion,
            EvaluatedAtUtc = DateTimeOffset.UtcNow,
        };
        assessment.ApplyCreatedDateTime();

        ApplyTerminologySlice(
            assessment,
            new TerminologyContext(correlationId, rid, cs, cv, uc, terminologyActive, tenantId));
        ApplyProfileSlice(
            assessment,
            new ProfileSliceContext(correlationId, rid, pu, profileActive, tenantId));

        return assessment;
    }

    private readonly record struct TerminologyContext(
        Ulid CorrelationId,
        string ResourceId,
        string? CodeSystemUri,
        string? CodeValue,
        string? UnitCode,
        bool TerminologyActive,
        string? TenantId);

    private readonly record struct ProfileSliceContext(
        Ulid CorrelationId,
        string ResourceId,
        string? ProfileUrl,
        bool ProfileActive,
        string? TenantId);

    private static void ApplyTerminologySlice(ConformanceAssessment assessment, TerminologyContext ctx)
    {
        if (!ctx.TerminologyActive)
        {
            assessment.TerminologySliceOutcome = ConformanceSliceOutcome.Skipped;
            assessment.TerminologyReason = null;
            return;
        }

        bool hasCodePart = !string.IsNullOrEmpty(ctx.CodeValue) || !string.IsNullOrEmpty(ctx.CodeSystemUri);
        bool hasUnitPart = !string.IsNullOrEmpty(ctx.UnitCode);
        if (hasCodePart && (string.IsNullOrEmpty(ctx.CodeValue) || string.IsNullOrEmpty(ctx.CodeSystemUri)))
        {
            assessment.TerminologySliceOutcome = ConformanceSliceOutcome.Failed;
            assessment.TerminologyReason = TruncateReason(
                "Code and code system URI must both be provided when validating a coded element.");
            assessment.ApplyEvent(
                new TerminologyValidationFailedIntegrationEvent(
                    ctx.CorrelationId,
                    ctx.ResourceId,
                    assessment.TerminologyReason)
                {
                    TenantId = ctx.TenantId,
                });
            return;
        }

        if (hasCodePart && string.Equals(ctx.CodeValue, "INVALID-CODE", StringComparison.OrdinalIgnoreCase))
        {
            assessment.TerminologySliceOutcome = ConformanceSliceOutcome.Failed;
            assessment.TerminologyReason = TruncateReason("Code is not allowed by the stub terminology registry.");
            assessment.ApplyEvent(
                new TerminologyValidationFailedIntegrationEvent(
                    ctx.CorrelationId,
                    ctx.ResourceId,
                    assessment.TerminologyReason)
                {
                    TenantId = ctx.TenantId,
                });
            return;
        }

        if (hasUnitPart && string.Equals(ctx.UnitCode, "invalid-ucum", StringComparison.OrdinalIgnoreCase))
        {
            assessment.TerminologySliceOutcome = ConformanceSliceOutcome.Failed;
            assessment.TerminologyReason = TruncateReason("Unit is not recognized as valid UCUM (stub check).");
            assessment.ApplyEvent(
                new TerminologyValidationFailedIntegrationEvent(
                    ctx.CorrelationId,
                    ctx.ResourceId,
                    assessment.TerminologyReason)
                {
                    TenantId = ctx.TenantId,
                });
            return;
        }

        assessment.TerminologySliceOutcome = ConformanceSliceOutcome.Passed;
        assessment.TerminologyReason = null;
        assessment.ApplyEvent(
            new TerminologyValidatedIntegrationEvent(ctx.CorrelationId, ctx.ResourceId)
            {
                TenantId = ctx.TenantId,
            });
    }

    private static void ApplyProfileSlice(ConformanceAssessment assessment, ProfileSliceContext ctx)
    {
        if (!ctx.ProfileActive)
        {
            assessment.ProfileSliceOutcome = ConformanceSliceOutcome.Skipped;
            assessment.ProfileReason = null;
            assessment.AssessedProfileUrl = null;
            return;
        }

        string profile = ctx.ProfileUrl!;
        assessment.AssessedProfileUrl = profile;
        if (profile.Contains("non-conformant", StringComparison.OrdinalIgnoreCase))
        {
            assessment.ProfileSliceOutcome = ConformanceSliceOutcome.Failed;
            assessment.ProfileReason = TruncateReason("Profile structure does not satisfy stub conformance rules.");
            assessment.ApplyEvent(
                new ProfileConformanceFailedIntegrationEvent(
                    ctx.CorrelationId,
                    ctx.ResourceId,
                    profile,
                    assessment.ProfileReason)
                {
                    TenantId = ctx.TenantId,
                });
            return;
        }

        assessment.ProfileSliceOutcome = ConformanceSliceOutcome.Passed;
        assessment.ProfileReason = null;
        assessment.ApplyEvent(
            new ProfileConformanceValidatedIntegrationEvent(ctx.CorrelationId, ctx.ResourceId, profile)
            {
                TenantId = ctx.TenantId,
            });
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
        string t = value.Trim();
        return t.Length <= maxLength ? t : t[..maxLength];
    }

    private static string TruncateReason(string reason) =>
        reason.Length <= MaxReasonLength ? reason : reason[..MaxReasonLength];
}
