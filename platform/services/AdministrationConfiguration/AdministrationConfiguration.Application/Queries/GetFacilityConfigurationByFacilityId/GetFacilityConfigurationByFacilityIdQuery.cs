using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetFacilityConfigurationByFacilityId;

public sealed record GetFacilityConfigurationByFacilityIdQuery(string FacilityId) : IQuery<FacilityConfigurationReadDto?>;
