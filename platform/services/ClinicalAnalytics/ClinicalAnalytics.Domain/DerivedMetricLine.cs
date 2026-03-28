using ClinicalAnalytics.Domain.ValueObjects;

namespace ClinicalAnalytics.Domain;

public sealed class DerivedMetricLine
{
    public Ulid Id { get; private set; }

    public Ulid SessionAnalysisId { get; private set; }

    public MetricCode Code { get; private set; } = null!;

    public string Value { get; private set; } = null!;

    public string? Unit { get; private set; }

    private DerivedMetricLine()
    {
    }

    internal static DerivedMetricLine Create(Ulid sessionAnalysisId, MetricCode code, string value, string? unit)
    {
        ArgumentNullException.ThrowIfNull(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return new DerivedMetricLine
        {
            Id = Ulid.NewUlid(),
            SessionAnalysisId = sessionAnalysisId,
            Code = code,
            Value = value.Trim(),
            Unit = string.IsNullOrWhiteSpace(unit) ? null : unit.Trim(),
        };
    }
}
