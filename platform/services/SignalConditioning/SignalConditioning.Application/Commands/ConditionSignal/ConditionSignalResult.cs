namespace SignalConditioning.Application.Commands.ConditionSignal;

public sealed record ConditionSignalResult(Ulid ConditioningResultId, bool IsDropout, int QualityScorePercent);
