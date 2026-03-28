using BuildingBlocks.Abstractions;

namespace ReplayRecovery.Domain.Abstractions;

public interface IReplayJobRepository : IRepository<ReplayRecovery.Domain.ReplayJob>
{
    Task<ReplayRecovery.Domain.ReplayJob?> GetByIdAsync(Ulid replayJobId, CancellationToken cancellationToken = default);

    Task<ReplayRecovery.Domain.ReplayJob?> GetByIdForUpdateAsync(
        Ulid replayJobId,
        CancellationToken cancellationToken = default);
}
