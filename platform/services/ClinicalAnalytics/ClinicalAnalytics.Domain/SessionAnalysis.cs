using BuildingBlocks;
using BuildingBlocks.ValueObjects;

using ClinicalAnalytics.Domain.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

namespace ClinicalAnalytics.Domain;

public sealed class SessionAnalysis : AggregateRoot
{
    private SessionAnalysis()
    {
    }

    public SessionId TreatmentSessionId { get; private set; }

    public ModelVersion AnalyticalModelVersion { get; private set; } = null!;

    public ConfidenceScore OverallConfidence { get; private set; }

    public InterpretationStatus Interpretation { get; private set; } = null!;

    public string TrendSummary { get; private set; } = null!;

    public List<DerivedMetricLine> DerivedMetrics { get; private set; } = [];

    public static SessionAnalysis RunMvpAnalysis(
        Ulid correlationId,
        SessionId treatmentSessionId,
        ModelVersion modelVersion,
        string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(modelVersion);
        var analysis = new SessionAnalysis
        {
            TreatmentSessionId = treatmentSessionId,
            AnalyticalModelVersion = modelVersion,
            OverallConfidence = ConfidenceScore.FromPercent(72),
            Interpretation = InterpretationStatus.PendingReview,
            TrendSummary = "Placeholder session trend summary (MVP; not for sole clinical decision-making).",
        };
        analysis.ApplyCreatedDateTime();
        Ulid analysisId = analysis.Id;
        string analysisIdStr = analysisId.ToString();
        string sessionStr = treatmentSessionId.Value;

        DerivedMetricLine m1 = DerivedMetricLine.Create(analysisId, new MetricCode("map.mean"), "82.3", "mmHg");
        DerivedMetricLine m2 = DerivedMetricLine.Create(analysisId, new MetricCode("map.trend_slope"), "0.12", "mmHg/min");
        analysis.DerivedMetrics.Add(m1);
        analysis.DerivedMetrics.Add(m2);

        analysis.ApplyEvent(
            new DerivedMetricCalculatedIntegrationEvent(correlationId, analysisIdStr, m1.Code.Value, m1.Value, m1.Unit)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
        analysis.ApplyEvent(
            new DerivedMetricCalculatedIntegrationEvent(correlationId, analysisIdStr, m2.Code.Value, m2.Value, m2.Unit)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
        analysis.ApplyEvent(
            new SessionTrendComputedIntegrationEvent(correlationId, analysisIdStr, analysis.TrendSummary)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
        analysis.ApplyEvent(
            new AnalyticalConfidenceAssignedIntegrationEvent(correlationId, analysisIdStr, analysis.OverallConfidence.Percent)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
        analysis.ApplyEvent(
            new SessionAnalysisCompletedIntegrationEvent(
                correlationId,
                analysisIdStr,
                modelVersion.Value,
                analysis.DerivedMetrics.Count)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });

        return analysis;
    }
}
