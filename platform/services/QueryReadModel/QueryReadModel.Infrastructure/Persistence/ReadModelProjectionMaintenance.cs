using QueryReadModel.Application.Abstractions;

using QueryReadModel.Domain;

using Microsoft.EntityFrameworkCore;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class ReadModelProjectionMaintenance : IReadModelProjectionMaintenance
{
    private readonly QueryReadModelDbContext _db;

    public ReadModelProjectionMaintenance(QueryReadModelDbContext db) =>
        _db = db ?? throw new ArgumentNullException(nameof(db));

    public async Task<int> ClearAndSeedStubAsync(CancellationToken cancellationToken = default)
    {
        _ = await _db.SessionOverviewProjections.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);
        _ = await _db.AlertProjections.ExecuteDeleteAsync(cancellationToken).ConfigureAwait(false);

        DateTimeOffset now = DateTimeOffset.UtcNow;
        SessionOverviewProjection session = SessionOverviewProjection.Create(
            "demo-session-1",
            "Active",
            "Demo Patient",
            "demo-device-1",
            now.AddHours(-2));
        AlertProjection alert = AlertProjection.Create(
            "demo-alert-1",
            "HYPOTENSION",
            "High",
            "Active",
            "demo-session-1",
            now.AddMinutes(-45));

        _ = await _db.SessionOverviewProjections.AddAsync(session, cancellationToken).ConfigureAwait(false);
        _ = await _db.AlertProjections.AddAsync(alert, cancellationToken).ConfigureAwait(false);

        return 2;
    }
}
