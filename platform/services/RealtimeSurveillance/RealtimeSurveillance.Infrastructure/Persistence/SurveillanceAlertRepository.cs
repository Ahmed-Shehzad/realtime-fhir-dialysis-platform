using BuildingBlocks;
using BuildingBlocks.ValueObjects;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace RealtimeSurveillance.Infrastructure.Persistence;

public sealed class SurveillanceAlertRepository : Repository<SurveillanceAlert>, ISurveillanceAlertRepository
{
    private readonly RealtimeSurveillanceDbContext _db;

    public SurveillanceAlertRepository(RealtimeSurveillanceDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<SurveillanceAlert?> GetByIdForUpdateAsync(Ulid alertId, CancellationToken cancellationToken = default) =>
        _db.SurveillanceAlerts.FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

    public Task<SurveillanceAlert?> GetByIdAsync(Ulid alertId, CancellationToken cancellationToken = default) =>
        _db.SurveillanceAlerts.AsNoTracking().FirstOrDefaultAsync(a => a.Id == alertId, cancellationToken);

    public async Task<IReadOnlyList<SurveillanceAlert>> ListBySessionAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return Array.Empty<SurveillanceAlert>();
        string sid = treatmentSessionId.Trim();
        var sessionId = new SessionId(sid);
        return await _db.SurveillanceAlerts
            .AsNoTracking()
            .Where(a => a.TreatmentSessionId == sessionId)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
