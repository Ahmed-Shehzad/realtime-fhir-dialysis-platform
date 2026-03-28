using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetFacilityConfigurationByFacilityId;

public sealed class GetFacilityConfigurationByFacilityIdQueryHandler
    : IQueryHandler<GetFacilityConfigurationByFacilityIdQuery, FacilityConfigurationReadDto?>
{
    private readonly IFacilityConfigurationRepository _configurations;

    public GetFacilityConfigurationByFacilityIdQueryHandler(IFacilityConfigurationRepository configurations) =>
        _configurations = configurations ?? throw new ArgumentNullException(nameof(configurations));

    public async Task<FacilityConfigurationReadDto?> HandleAsync(
        GetFacilityConfigurationByFacilityIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var facilityId = new FacilityId(query.FacilityId);
        FacilityConfiguration? row = await _configurations.GetByFacilityIdAsync(facilityId, cancellationToken).ConfigureAwait(false);
        if (row is null)
            return null;
        return new FacilityConfigurationReadDto(
            row.Id.ToString(),
            row.FacilityId.Value,
            row.Configuration.Json,
            row.ConfigurationRevision,
            row.EffectiveFromUtc,
            row.EffectiveToUtc);
    }
}
