using QueryReadModel.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class DashboardReadModelQuery : IDashboardReadModelQuery
{
    private readonly QueryReadModelDbContext _db;

    public DashboardReadModelQuery(QueryReadModelDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    public Task<int> CountActiveSessionsAsync(CancellationToken cancellationToken = default) =>
        _db.SessionOverviewProjections
            .AsNoTracking()
            .CountAsync(s => EF.Functions.ILike(s.SessionState, "active"), cancellationToken);
}
