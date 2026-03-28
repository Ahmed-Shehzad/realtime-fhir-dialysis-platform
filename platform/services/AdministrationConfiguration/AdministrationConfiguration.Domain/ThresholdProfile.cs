using BuildingBlocks;

using AdministrationConfiguration.Domain.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

namespace AdministrationConfiguration.Domain;

public sealed class ThresholdProfile : AggregateRoot
{
    private ThresholdProfile()
    {
    }

    public ThresholdProfileCode ProfileCode { get; private set; } = null!;

    public ThresholdProfilePayload Payload { get; private set; }

    public int ProfileRevision { get; private set; }

    public static ThresholdProfile Create(
        Ulid correlationId,
        ThresholdProfileCode code,
        ThresholdProfilePayload payload,
        string? tenantId)
    {
        var profile = new ThresholdProfile
        {
            ProfileCode = code,
            Payload = payload,
            ProfileRevision = 1,
        };
        profile.ApplyCreatedDateTime();
        profile.ApplyEvent(
            new ThresholdProfileChangedIntegrationEvent(correlationId, profile.Id.ToString(), profile.ProfileRevision)
            {
                TenantId = tenantId,
            });
        return profile;
    }

    public void ReplacePayload(Ulid correlationId, ThresholdProfilePayload payload, string? tenantId)
    {
        Payload = payload;
        ProfileRevision = checked(ProfileRevision + 1);
        ApplyUpdateDateTime();
        ApplyEvent(
            new ThresholdProfileChangedIntegrationEvent(correlationId, Id.ToString(), ProfileRevision)
            {
                TenantId = tenantId,
            });
    }
}
