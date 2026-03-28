using BuildingBlocks;

using AdministrationConfiguration.Domain.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

namespace AdministrationConfiguration.Domain;

public sealed class FeatureToggle : AggregateRoot
{
    private FeatureToggle()
    {
    }

    public FeatureFlagKey FeatureKey { get; private set; } = null!;

    public bool IsEnabled { get; private set; }

    public static FeatureToggle Create(Ulid correlationId, FeatureFlagKey key, bool isEnabled, string? tenantId)
    {
        var toggle = new FeatureToggle
        {
            FeatureKey = key,
            IsEnabled = isEnabled,
        };
        toggle.ApplyCreatedDateTime();
        toggle.ApplyEvent(
            new FeatureToggleChangedIntegrationEvent(correlationId, key.Value, isEnabled)
            {
                TenantId = tenantId,
            });
        return toggle;
    }

    public void SetEnabled(Ulid correlationId, bool isEnabled, string? tenantId)
    {
        if (IsEnabled == isEnabled)
            return;
        IsEnabled = isEnabled;
        ApplyUpdateDateTime();
        ApplyEvent(
            new FeatureToggleChangedIntegrationEvent(correlationId, FeatureKey.Value, isEnabled)
            {
                TenantId = tenantId,
            });
    }
}
