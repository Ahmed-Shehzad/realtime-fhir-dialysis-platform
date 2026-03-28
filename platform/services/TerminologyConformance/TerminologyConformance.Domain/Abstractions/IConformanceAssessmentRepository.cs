using BuildingBlocks.Abstractions;

namespace TerminologyConformance.Domain.Abstractions;

public interface IConformanceAssessmentRepository : IRepository<TerminologyConformance.Domain.ConformanceAssessment>
{
    Task<TerminologyConformance.Domain.ConformanceAssessment?> GetLatestByResourceIdAsync(string resourceId, CancellationToken cancellationToken = default);
}
