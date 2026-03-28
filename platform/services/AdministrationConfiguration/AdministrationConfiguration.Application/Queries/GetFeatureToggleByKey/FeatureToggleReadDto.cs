namespace AdministrationConfiguration.Application.Queries.GetFeatureToggleByKey;

public sealed record FeatureToggleReadDto(string Id, string FeatureKey, bool IsEnabled);
