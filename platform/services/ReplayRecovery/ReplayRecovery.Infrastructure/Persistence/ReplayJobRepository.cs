using Microsoft.EntityFrameworkCore;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;

using BuildingBlocks;

namespace ReplayRecovery.Infrastructure.Persistence;

public sealed class ReplayJobRepository : Repository<ReplayJob>, IReplayJobRepository
{
    private readonly ReplayRecoveryDbContext _db;

    public ReplayJobRepository(ReplayRecoveryDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<ReplayJob?> GetByIdAsync(Ulid replayJobId, CancellationToken cancellationToken = default) =>
        _db.ReplayJobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == replayJobId, cancellationToken);

    public Task<ReplayJob?> GetByIdForUpdateAsync(Ulid replayJobId, CancellationToken cancellationToken = default) =>
        _db.ReplayJobs.FirstOrDefaultAsync(j => j.Id == replayJobId, cancellationToken);
}
