using BuildingBlocks;

using BuildingBlocks.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Domain;

public sealed class SessionRiskSnapshot : AggregateRoot
{
    private SessionRiskSnapshot()
    {
    }

    public SessionId TreatmentSessionId { get; private set; }

    public SessionRiskLevel RiskLevel { get; private set; } = null!;

    public static SessionRiskSnapshot Start(Ulid correlationId, SessionId sessionId, SessionRiskLevel level, string? tenantId)
    {
        var row = new SessionRiskSnapshot
        {
            TreatmentSessionId = sessionId,
            RiskLevel = level,
        };
        row.ApplyCreatedDateTime();
        row.ApplyRiskChanged(correlationId, tenantId);
        return row;
    }

    public void UpdateRisk(Ulid correlationId, SessionRiskLevel newLevel, string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(newLevel);
        if (RiskLevel == newLevel)
            return;
        RiskLevel = newLevel;
        ApplyUpdateDateTime();
        ApplyRiskChanged(correlationId, tenantId);
    }

    private void ApplyRiskChanged(Ulid correlationId, string? tenantId) =>
        ApplyEvent(
            new SessionRiskStateChangedIntegrationEvent(correlationId, TreatmentSessionId.Value, RiskLevel.Value)
            {
                SessionId = TreatmentSessionId.Value,
                TenantId = tenantId,
            });
}
