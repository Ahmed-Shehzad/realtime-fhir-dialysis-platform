using Microsoft.EntityFrameworkCore;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using BuildingBlocks;

namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class FeatureToggleRepository : Repository<FeatureToggle>, IFeatureToggleRepository
{
    private readonly AdministrationConfigurationDbContext _db;

    public FeatureToggleRepository(AdministrationConfigurationDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<FeatureToggle?> GetByFeatureKeyAsync(FeatureFlagKey key, CancellationToken cancellationToken = default) =>
        _db.FeatureToggles.AsNoTracking().FirstOrDefaultAsync(t => t.FeatureKey == key, cancellationToken);

    public Task<FeatureToggle?> GetByFeatureKeyForUpdateAsync(FeatureFlagKey key, CancellationToken cancellationToken = default) =>
        _db.FeatureToggles.FirstOrDefaultAsync(t => t.FeatureKey == key, cancellationToken);
}
