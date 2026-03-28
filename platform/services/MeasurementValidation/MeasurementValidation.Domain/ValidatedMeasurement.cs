using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace MeasurementValidation.Domain;

/// <summary>Validation decision for a measurement (MVP rules engine).</summary>
public sealed class ValidatedMeasurement : AggregateRoot
{
    public const int MaxMeasurementIdLength = 256;

    public const int MaxValidationProfileIdLength = 256;

    public const int MaxReasonLength = 2000;

    public const string DefaultRuleSetVersion = "rules-v1";

    private ValidatedMeasurement()
    {
    }

    public string MeasurementId { get; private set; } = null!;

    public string ValidationProfileId { get; private set; } = null!;

    public MeasurementValidationOutcome Outcome { get; private set; }

    public string? Reason { get; private set; }

    public string RuleSetVersion { get; private set; } = null!;

    public DateTimeOffset EvaluatedAtUtc { get; private set; }

    public static ValidatedMeasurement Run(
        Ulid correlationId,
        string measurementId,
        string validationProfileId,
        double? sampleValue,
        string? tenantId)
    {
        string mid = (measurementId ?? string.Empty).Trim();
        if (mid.Length == 0 || mid.Length > MaxMeasurementIdLength)
            throw new ArgumentException("MeasurementId is invalid.", nameof(measurementId));

        string pid = (validationProfileId ?? string.Empty).Trim();
        if (pid.Length == 0 || pid.Length > MaxValidationProfileIdLength)
            throw new ArgumentException("ValidationProfileId is invalid.", nameof(validationProfileId));

        var validation = new ValidatedMeasurement
        {
            MeasurementId = mid,
            ValidationProfileId = pid,
            RuleSetVersion = DefaultRuleSetVersion,
            EvaluatedAtUtc = DateTimeOffset.UtcNow,
        };
        validation.ApplyCreatedDateTime();

        void TriggerRule(string ruleCode, string outcome) =>
            validation.ApplyEvent(
                new ValidationRuleTriggeredIntegrationEvent(correlationId, mid, ruleCode, outcome)
                {
                    TenantId = tenantId,
                });

        if (sampleValue is null)
        {
            validation.Outcome = MeasurementValidationOutcome.Quarantined;
            validation.Reason = TruncateReason("Sample value is required for validation.");
            TriggerRule("RULE_SAMPLE_REQUIRED", "Quarantined");
            validation.ApplyEvent(
                new MeasurementValidationQuarantinedIntegrationEvent(correlationId, mid, pid, validation.Reason)
                {
                    TenantId = tenantId,
                });
            return validation;
        }

        double x = sampleValue.Value;
        if (x < 0d)
        {
            validation.Outcome = MeasurementValidationOutcome.Failed;
            validation.Reason = TruncateReason("Value is below the allowed minimum.");
            TriggerRule("RULE_RANGE_MIN", "Failed");
            validation.ApplyEvent(
                new MeasurementValidationFailedIntegrationEvent(correlationId, mid, pid, validation.Reason)
                {
                    TenantId = tenantId,
                });
            return validation;
        }

        if (x > 1000d)
        {
            validation.Outcome = MeasurementValidationOutcome.Failed;
            validation.Reason = TruncateReason("Value exceeds the allowed maximum.");
            TriggerRule("RULE_RANGE_MAX", "Failed");
            validation.ApplyEvent(
                new MeasurementValidationFailedIntegrationEvent(correlationId, mid, pid, validation.Reason)
                {
                    TenantId = tenantId,
                });
            return validation;
        }

        if (x > 500d)
        {
            validation.Outcome = MeasurementValidationOutcome.Flagged;
            validation.Reason = TruncateReason("Value is in the elevated band; manual review recommended.");
            TriggerRule("RULE_ELEVATED_BAND", "Flagged");
            validation.ApplyEvent(
                new MeasurementFlaggedIntegrationEvent(correlationId, mid, pid, validation.Reason)
                {
                    TenantId = tenantId,
                });
            validation.ApplyEvent(
                new MeasurementValidatedIntegrationEvent(correlationId, mid, pid)
                {
                    TenantId = tenantId,
                });
            return validation;
        }

        validation.Outcome = MeasurementValidationOutcome.Passed;
        validation.Reason = null;
        TriggerRule("RULE_WITHIN_RANGE", "Passed");
        validation.ApplyEvent(
            new MeasurementValidatedIntegrationEvent(correlationId, mid, pid)
            {
                TenantId = tenantId,
            });
        return validation;
    }

    private static string TruncateReason(string reason) =>
        reason.Length <= MaxReasonLength ? reason : reason[..MaxReasonLength];
}
