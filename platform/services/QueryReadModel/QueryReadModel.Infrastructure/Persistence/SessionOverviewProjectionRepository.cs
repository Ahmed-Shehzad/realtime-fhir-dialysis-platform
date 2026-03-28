using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class SessionOverviewProjectionRepository : Repository<SessionOverviewProjection>, ISessionOverviewProjectionRepository
{
    private readonly QueryReadModelDbContext _db;

    public SessionOverviewProjectionRepository(QueryReadModelDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<SessionOverviewProjection?> GetByTreatmentSessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return null;
        string id = treatmentSessionId.Trim();
        return await _db.SessionOverviewProjections
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.TreatmentSessionId == id, cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<SessionOverviewProjection?> GetByTreatmentSessionIdForUpdateAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return Task.FromResult<SessionOverviewProjection?>(null);
        string id = treatmentSessionId.Trim();
        return _db.SessionOverviewProjections.FirstOrDefaultAsync(s => s.TreatmentSessionId == id, cancellationToken);
    }
}
