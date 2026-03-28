using Microsoft.EntityFrameworkCore;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using BuildingBlocks;

namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class FacilityConfigurationRepository : Repository<FacilityConfiguration>, IFacilityConfigurationRepository
{
    private readonly AdministrationConfigurationDbContext _db;

    public FacilityConfigurationRepository(AdministrationConfigurationDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<FacilityConfiguration?> GetByFacilityIdAsync(FacilityId facilityId, CancellationToken cancellationToken = default) =>
        _db.FacilityConfigurations.AsNoTracking().FirstOrDefaultAsync(c => c.FacilityId == facilityId, cancellationToken);

    public Task<FacilityConfiguration?> GetByFacilityIdForUpdateAsync(
        FacilityId facilityId,
        CancellationToken cancellationToken = default) =>
        _db.FacilityConfigurations.FirstOrDefaultAsync(c => c.FacilityId == facilityId, cancellationToken);
}
