using AdministrationConfiguration.Domain.ValueObjects;

using BuildingBlocks.Abstractions;

namespace AdministrationConfiguration.Domain.Abstractions;

public interface IFacilityConfigurationRepository : IRepository<FacilityConfiguration>
{
    Task<FacilityConfiguration?> GetByFacilityIdAsync(FacilityId facilityId, CancellationToken cancellationToken = default);

    Task<FacilityConfiguration?> GetByFacilityIdForUpdateAsync(
        FacilityId facilityId,
        CancellationToken cancellationToken = default);
}
