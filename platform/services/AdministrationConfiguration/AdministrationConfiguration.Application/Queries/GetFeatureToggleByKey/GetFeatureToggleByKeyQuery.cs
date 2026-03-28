using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetFeatureToggleByKey;

public sealed record GetFeatureToggleByKeyQuery(string FeatureKey) : IQuery<FeatureToggleReadDto?>;
