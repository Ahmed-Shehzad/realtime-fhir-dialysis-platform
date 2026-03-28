using AdministrationConfiguration.Domain.ValueObjects;

using BuildingBlocks.Abstractions;

namespace AdministrationConfiguration.Domain.Abstractions;

public interface IFeatureToggleRepository : IRepository<FeatureToggle>
{
    Task<FeatureToggle?> GetByFeatureKeyAsync(FeatureFlagKey key, CancellationToken cancellationToken = default);

    Task<FeatureToggle?> GetByFeatureKeyForUpdateAsync(FeatureFlagKey key, CancellationToken cancellationToken = default);
}
