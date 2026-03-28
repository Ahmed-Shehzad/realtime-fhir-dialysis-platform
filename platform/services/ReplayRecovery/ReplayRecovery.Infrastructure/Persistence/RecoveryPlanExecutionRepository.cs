using Microsoft.EntityFrameworkCore;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;

using BuildingBlocks;

namespace ReplayRecovery.Infrastructure.Persistence;

public sealed class RecoveryPlanExecutionRepository : Repository<RecoveryPlanExecution>, IRecoveryPlanExecutionRepository
{
    private readonly ReplayRecoveryDbContext _db;

    public RecoveryPlanExecutionRepository(ReplayRecoveryDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<RecoveryPlanExecution?> GetByIdAsync(Ulid executionId, CancellationToken cancellationToken = default) =>
        _db.RecoveryPlanExecutions.AsNoTracking().FirstOrDefaultAsync(e => e.Id == executionId, cancellationToken);
}
