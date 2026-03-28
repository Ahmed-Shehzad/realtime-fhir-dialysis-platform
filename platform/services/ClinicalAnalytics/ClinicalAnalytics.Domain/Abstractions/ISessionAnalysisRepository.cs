using BuildingBlocks.Abstractions;

namespace ClinicalAnalytics.Domain.Abstractions;

public interface ISessionAnalysisRepository : IRepository<ClinicalAnalytics.Domain.SessionAnalysis>
{
    Task<ClinicalAnalytics.Domain.SessionAnalysis?> GetByIdAsync(
        Ulid analysisId,
        CancellationToken cancellationToken = default);
}
