using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace SignalConditioning.Domain;

/// <summary>MVP conditioning pass: dropout detection, simple drift vs previous sample, quality score, conditioned kind.</summary>
public sealed class ConditioningResult : AggregateRoot
{
    public const int MaxMeasurementIdLength = 256;

    public const int MaxChannelIdLength = 128;

    public const string DefaultMethodVersion = "conditioning-v1";

    public const double DriftDeltaThreshold = 50d;

    public const double QualityMidReference = 500d;

    public const string StandardConditionedKind = "StandardScalar";

    private ConditioningResult()
    {
    }

    public string MeasurementId { get; private set; } = null!;

    public string ChannelId { get; private set; } = null!;

    public bool IsDropout { get; private set; }

    public bool DriftDetected { get; private set; }

    public int QualityScorePercent { get; private set; }

    public string ConditioningMethodVersion { get; private set; } = null!;

    public string? ConditionedSignalKind { get; private set; }

    public DateTimeOffset EvaluatedAtUtc { get; private set; }

    public static ConditioningResult Run(
        Ulid correlationId,
        string measurementId,
        string channelId,
        double? sampleValue,
        double? previousSampleValue,
        string? tenantId)
    {
        string mid = (measurementId ?? string.Empty).Trim();
        if (mid.Length == 0 || mid.Length > MaxMeasurementIdLength)
            throw new ArgumentException("MeasurementId is invalid.", nameof(measurementId));

        string cid = (channelId ?? string.Empty).Trim();
        if (cid.Length == 0 || cid.Length > MaxChannelIdLength)
            throw new ArgumentException("ChannelId is invalid.", nameof(channelId));

        var result = new ConditioningResult
        {
            MeasurementId = mid,
            ChannelId = cid,
            ConditioningMethodVersion = DefaultMethodVersion,
            EvaluatedAtUtc = DateTimeOffset.UtcNow,
        };
        result.ApplyCreatedDateTime();

        if (sampleValue is null)
        {
            result.IsDropout = true;
            result.QualityScorePercent = 0;
            result.ApplyEvent(
                new SignalDropoutDetectedIntegrationEvent(correlationId, mid, cid)
                {
                    TenantId = tenantId,
                });
            return result;
        }

        result.IsDropout = false;
        double sample = sampleValue.Value;
        int quality = (int)Math.Clamp(100d - Math.Abs(sample - QualityMidReference) / 10d, 0d, 100d);
        result.QualityScorePercent = quality;

        if (previousSampleValue is not null
            && Math.Abs(sample - previousSampleValue.Value) > DriftDeltaThreshold)
        {
            result.DriftDetected = true;
            result.ApplyEvent(
                new SignalDriftDetectedIntegrationEvent(correlationId, mid, cid, "Step delta exceeds threshold.")
                {
                    TenantId = tenantId,
                });
        }

        result.ApplyEvent(
            new SignalQualityCalculatedIntegrationEvent(correlationId, mid, quality)
            {
                TenantId = tenantId,
            });
        result.ConditionedSignalKind = StandardConditionedKind;
        result.ApplyEvent(
            new SignalConditionedIntegrationEvent(correlationId, mid, StandardConditionedKind)
            {
                TenantId = tenantId,
            });
        return result;
    }
}
