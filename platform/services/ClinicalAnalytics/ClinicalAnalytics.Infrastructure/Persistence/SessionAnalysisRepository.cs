using ClinicalAnalytics.Domain;
using ClinicalAnalytics.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace ClinicalAnalytics.Infrastructure.Persistence;

public sealed class SessionAnalysisRepository : Repository<SessionAnalysis>, ISessionAnalysisRepository
{
    private readonly ClinicalAnalyticsDbContext _db;

    public SessionAnalysisRepository(ClinicalAnalyticsDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<SessionAnalysis?> GetByIdAsync(Ulid analysisId, CancellationToken cancellationToken = default) =>
        _db.SessionAnalyses
            .AsSplitQuery()
            .AsNoTracking()
            .Include(a => a.DerivedMetrics)
            .FirstOrDefaultAsync(a => a.Id == analysisId, cancellationToken);
}
