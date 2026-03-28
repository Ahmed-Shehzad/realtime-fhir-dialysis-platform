namespace AdministrationConfiguration.Application.Queries.GetFacilityConfigurationByFacilityId;

public sealed record FacilityConfigurationReadDto(
    string Id,
    string FacilityId,
    string ConfigurationJson,
    int ConfigurationRevision,
    DateTimeOffset? EffectiveFromUtc,
    DateTimeOffset? EffectiveToUtc);
