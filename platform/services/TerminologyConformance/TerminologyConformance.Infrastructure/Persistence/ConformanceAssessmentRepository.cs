using TerminologyConformance.Domain;
using TerminologyConformance.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace TerminologyConformance.Infrastructure.Persistence;

public sealed class ConformanceAssessmentRepository : Repository<ConformanceAssessment>, IConformanceAssessmentRepository
{
    private readonly TerminologyConformanceDbContext _db;

    public ConformanceAssessmentRepository(TerminologyConformanceDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<ConformanceAssessment?> GetLatestByResourceIdAsync(
        string resourceId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(resourceId))
            return null;
        string id = resourceId.Trim();
        return await _db.ConformanceAssessments
            .AsNoTracking()
            .Where(a => a.ResourceId == id)
            .OrderByDescending(a => a.EvaluatedAtUtc)
            .ThenByDescending(a => a.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
