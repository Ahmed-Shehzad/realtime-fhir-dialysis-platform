using ClinicalAnalytics.Domain;
using ClinicalAnalytics.Domain.Abstractions;

using Intercessor.Abstractions;

namespace ClinicalAnalytics.Application.Queries.GetSessionAnalysisById;

public sealed class GetSessionAnalysisByIdQueryHandler : IQueryHandler<GetSessionAnalysisByIdQuery, SessionAnalysisReadDto?>
{
    private readonly ISessionAnalysisRepository _analyses;

    public GetSessionAnalysisByIdQueryHandler(ISessionAnalysisRepository analyses) =>
        _analyses = analyses ?? throw new ArgumentNullException(nameof(analyses));

    public async Task<SessionAnalysisReadDto?> HandleAsync(
        GetSessionAnalysisByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        SessionAnalysis? row = await _analyses
            .GetByIdAsync(query.AnalysisId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;

        var metrics = row.DerivedMetrics
            .Select(m => new DerivedMetricReadDto(m.Code.Value, m.Value, m.Unit))
            .ToList();

        return new SessionAnalysisReadDto(
            row.Id.ToString(),
            row.TreatmentSessionId.Value,
            row.AnalyticalModelVersion.Value,
            row.OverallConfidence.Percent,
            row.Interpretation.Value,
            row.TrendSummary,
            metrics,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
