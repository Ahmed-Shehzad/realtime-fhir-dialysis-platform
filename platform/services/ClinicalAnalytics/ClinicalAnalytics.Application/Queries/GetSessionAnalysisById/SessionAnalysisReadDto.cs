namespace ClinicalAnalytics.Application.Queries.GetSessionAnalysisById;

public sealed record SessionAnalysisReadDto(
    string AnalysisId,
    string TreatmentSessionId,
    string ModelVersion,
    int ConfidencePercent,
    string Interpretation,
    string TrendSummary,
    IReadOnlyList<DerivedMetricReadDto> Metrics,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);

public sealed record DerivedMetricReadDto(string MetricCode, string Value, string? Unit);
