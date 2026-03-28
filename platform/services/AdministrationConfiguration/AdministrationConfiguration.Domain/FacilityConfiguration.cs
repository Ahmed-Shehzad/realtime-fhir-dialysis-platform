using BuildingBlocks;

using AdministrationConfiguration.Domain.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

namespace AdministrationConfiguration.Domain;

public sealed class FacilityConfiguration : AggregateRoot
{
    private FacilityConfiguration()
    {
    }

    public FacilityId FacilityId { get; private set; } = null!;

    public ConfigurationPayload Configuration { get; private set; }

    public int ConfigurationRevision { get; private set; }

    public DateTimeOffset? EffectiveFromUtc { get; private set; }

    public DateTimeOffset? EffectiveToUtc { get; private set; }

    public static FacilityConfiguration Create(
        Ulid correlationId,
        FacilityId facilityId,
        ConfigurationPayload configuration,
        EffectiveDateRange? effectiveRange,
        string? tenantId)
    {
        effectiveRange?.ThrowIfInvalid();

        var aggregate = new FacilityConfiguration
        {
            FacilityId = facilityId,
            Configuration = configuration,
            ConfigurationRevision = 1,
            EffectiveFromUtc = effectiveRange?.EffectiveFromUtc,
            EffectiveToUtc = effectiveRange?.EffectiveToUtc,
        };
        aggregate.ApplyCreatedDateTime();
        aggregate.ApplyEvent(
            new FacilityConfigurationChangedIntegrationEvent(
                correlationId,
                facilityId.Value,
                aggregate.ConfigurationRevision)
            {
                FacilityId = facilityId.Value,
                TenantId = tenantId,
            });
        return aggregate;
    }

    public void Update(
        Ulid correlationId,
        ConfigurationPayload configuration,
        EffectiveDateRange? effectiveRange,
        string? tenantId)
    {
        effectiveRange?.ThrowIfInvalid();
        Configuration = configuration;
        ConfigurationRevision = checked(ConfigurationRevision + 1);
        EffectiveFromUtc = effectiveRange?.EffectiveFromUtc;
        EffectiveToUtc = effectiveRange?.EffectiveToUtc;
        ApplyUpdateDateTime();
        ApplyEvent(
            new FacilityConfigurationChangedIntegrationEvent(correlationId, FacilityId.Value, ConfigurationRevision)
            {
                FacilityId = FacilityId.Value,
                TenantId = tenantId,
            });
    }
}
