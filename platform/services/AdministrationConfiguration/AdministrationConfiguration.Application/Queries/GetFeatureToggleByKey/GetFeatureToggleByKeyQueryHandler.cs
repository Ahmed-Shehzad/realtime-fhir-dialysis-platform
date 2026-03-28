using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;
using AdministrationConfiguration.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetFeatureToggleByKey;

public sealed class GetFeatureToggleByKeyQueryHandler
    : IQueryHandler<GetFeatureToggleByKeyQuery, FeatureToggleReadDto?>
{
    private readonly IFeatureToggleRepository _toggles;

    public GetFeatureToggleByKeyQueryHandler(IFeatureToggleRepository toggles) =>
        _toggles = toggles ?? throw new ArgumentNullException(nameof(toggles));

    public async Task<FeatureToggleReadDto?> HandleAsync(
        GetFeatureToggleByKeyQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var key = new FeatureFlagKey(query.FeatureKey);
        FeatureToggle? row = await _toggles.GetByFeatureKeyAsync(key, cancellationToken).ConfigureAwait(false);
        if (row is null)
            return null;
        return new FeatureToggleReadDto(row.Id.ToString(), row.FeatureKey.Value, row.IsEnabled);
    }
}
