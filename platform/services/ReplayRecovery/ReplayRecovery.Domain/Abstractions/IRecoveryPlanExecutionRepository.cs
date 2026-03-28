using BuildingBlocks.Abstractions;

namespace ReplayRecovery.Domain.Abstractions;

public interface IRecoveryPlanExecutionRepository : IRepository<ReplayRecovery.Domain.RecoveryPlanExecution>
{
    Task<ReplayRecovery.Domain.RecoveryPlanExecution?> GetByIdAsync(
        Ulid executionId,
        CancellationToken cancellationToken = default);
}
