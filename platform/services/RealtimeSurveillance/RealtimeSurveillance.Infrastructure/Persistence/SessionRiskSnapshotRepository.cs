using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace RealtimeSurveillance.Infrastructure.Persistence;

public sealed class SessionRiskSnapshotRepository : Repository<SessionRiskSnapshot>, ISessionRiskSnapshotRepository
{
    private readonly RealtimeSurveillanceDbContext _db;

    public SessionRiskSnapshotRepository(RealtimeSurveillanceDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<SessionRiskSnapshot?> GetBySessionIdForUpdateAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return Task.FromResult<SessionRiskSnapshot?>(null);
        string sid = treatmentSessionId.Trim();
        return _db.SessionRiskSnapshots.FirstOrDefaultAsync(r => r.TreatmentSessionId.Value == sid, cancellationToken);
    }

    public Task<SessionRiskSnapshot?> GetBySessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return Task.FromResult<SessionRiskSnapshot?>(null);
        string sid = treatmentSessionId.Trim();
        return _db.SessionRiskSnapshots.AsNoTracking().FirstOrDefaultAsync(r => r.TreatmentSessionId.Value == sid, cancellationToken);
    }
}
