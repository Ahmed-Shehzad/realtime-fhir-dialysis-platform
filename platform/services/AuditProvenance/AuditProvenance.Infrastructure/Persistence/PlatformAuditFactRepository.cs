using AuditProvenance.Domain;
using AuditProvenance.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace AuditProvenance.Infrastructure.Persistence;

public sealed class PlatformAuditFactRepository : Repository<PlatformAuditFact>, IPlatformAuditFactRepository
{
    private readonly AuditProvenanceDbContext _db;

    public PlatformAuditFactRepository(AuditProvenanceDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<PlatformAuditFact?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
        _db.PlatformAuditFacts.AsNoTracking().FirstOrDefaultAsync(f => f.Id == id, cancellationToken);

    public async Task<IReadOnlyList<PlatformAuditFactSummary>> GetRecentSummariesAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        int take = Math.Clamp(count, 1, 10_000);
        List<PlatformAuditFact> rows = await _db.PlatformAuditFacts
            .AsNoTracking()
            .OrderByDescending(f => f.OccurredAtUtc)
            .Take(take)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        List<PlatformAuditFactSummary> result = new(rows.Count);
        foreach (PlatformAuditFact f in rows) result.Add(new PlatformAuditFactSummary(f.Id, f.OccurredAtUtc, f.EventType, f.Summary, f.SourceSystem));

        return result;
    }
}
