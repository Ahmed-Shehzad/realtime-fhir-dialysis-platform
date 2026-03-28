namespace TerminologyConformance.Domain.Abstractions;

public sealed record ConformanceAssessmentSummary(
    Ulid Id,
    string ResourceId,
    ConformanceSliceOutcome TerminologyOutcome,
    ConformanceSliceOutcome ProfileOutcome,
    string? TerminologyReason,
    string? ProfileReason,
    string? AssessedProfileUrl,
    string ProfileRuleRegistryVersion,
    DateTimeOffset EvaluatedAtUtc);
