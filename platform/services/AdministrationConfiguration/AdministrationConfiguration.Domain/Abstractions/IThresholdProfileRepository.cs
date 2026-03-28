using BuildingBlocks.Abstractions;

namespace AdministrationConfiguration.Domain.Abstractions;

public interface IThresholdProfileRepository : IRepository<ThresholdProfile>
{
    Task<ThresholdProfile?> GetByIdAsync(Ulid profileId, CancellationToken cancellationToken = default);

    Task<ThresholdProfile?> GetByIdForUpdateAsync(Ulid profileId, CancellationToken cancellationToken = default);
}
