namespace MeasurementValidation.Domain.Abstractions;

public sealed record MeasurementValidationSummary(
    Ulid Id,
    string MeasurementId,
    string ValidationProfileId,
    global::MeasurementValidation.Domain.MeasurementValidationOutcome Outcome,
    string? Reason,
    string RuleSetVersion,
    DateTimeOffset EvaluatedAtUtc);
