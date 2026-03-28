using Microsoft.EntityFrameworkCore;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;

using BuildingBlocks;

namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class ThresholdProfileRepository : Repository<ThresholdProfile>, IThresholdProfileRepository
{
    private readonly AdministrationConfigurationDbContext _db;

    public ThresholdProfileRepository(AdministrationConfigurationDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<ThresholdProfile?> GetByIdAsync(Ulid profileId, CancellationToken cancellationToken = default) =>
        _db.ThresholdProfiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);

    public Task<ThresholdProfile?> GetByIdForUpdateAsync(Ulid profileId, CancellationToken cancellationToken = default) =>
        _db.ThresholdProfiles.FirstOrDefaultAsync(p => p.Id == profileId, cancellationToken);
}
