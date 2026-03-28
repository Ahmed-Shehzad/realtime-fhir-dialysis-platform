namespace SignalConditioning.Domain.Abstractions;

public sealed record ConditioningSummary(
    Ulid Id,
    string MeasurementId,
    string ChannelId,
    bool IsDropout,
    bool DriftDetected,
    int QualityScorePercent,
    string ConditioningMethodVersion,
    string? ConditionedSignalKind,
    DateTimeOffset EvaluatedAtUtc);
