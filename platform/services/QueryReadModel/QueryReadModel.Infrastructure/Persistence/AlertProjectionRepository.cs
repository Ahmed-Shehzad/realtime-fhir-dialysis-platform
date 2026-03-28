using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class AlertProjectionRepository : Repository<AlertProjection>, IAlertProjectionRepository
{
    private readonly QueryReadModelDbContext _db;

    public AlertProjectionRepository(QueryReadModelDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<AlertProjection?> GetByAlertRowKeyForUpdateAsync(
        string alertRowKey,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(alertRowKey))
            return Task.FromResult<AlertProjection?>(null);
        string key = alertRowKey.Trim();
        return _db.AlertProjections.FirstOrDefaultAsync(a => a.AlertRowKey == key, cancellationToken);
    }

    public async Task<IReadOnlyList<AlertProjection>> ListAsync(
        string? severityFilter,
        CancellationToken cancellationToken = default)
    {
        IQueryable<AlertProjection> q = _db.AlertProjections.AsNoTracking().OrderByDescending(a => a.RaisedAtUtc);
        if (!string.IsNullOrWhiteSpace(severityFilter))
        {
            string sev = severityFilter.Trim();
            q = q.Where(a => a.Severity == sev);
        }

        return await q.ToListAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> CountOpenAsync(CancellationToken cancellationToken = default)
    {
        return await _db.AlertProjections
            .AsNoTracking()
            .CountAsync(
                a => a.AlertState == null || !EF.Functions.ILike(a.AlertState, "cleared"),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
