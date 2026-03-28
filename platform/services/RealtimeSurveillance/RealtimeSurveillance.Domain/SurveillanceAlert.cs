using BuildingBlocks;

using BuildingBlocks.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Domain;

public sealed class SurveillanceAlert : AggregateRoot
{
    public const int MaxDetailLength = 1024;

    private SurveillanceAlert()
    {
    }

    public SessionId TreatmentSessionId { get; private set; }

    public AlertTypeCode AlertType { get; private set; }

    public AlertSeverityLevel Severity { get; private set; }

    public AlertLifecycleState LifecycleState { get; private set; } = null!;

    public string? Detail { get; private set; }

    public static SurveillanceAlert Raise(
        Ulid correlationId,
        SessionId treatmentSessionId,
        AlertTypeCode alertType,
        AlertSeverityLevel severity,
        string? detail,
        string? tenantId)
    {
        var alert = new SurveillanceAlert
        {
            TreatmentSessionId = treatmentSessionId,
            AlertType = alertType,
            Severity = severity,
            LifecycleState = AlertLifecycleState.Active,
            Detail = TruncateDetail(detail),
        };
        alert.ApplyCreatedDateTime();
        alert.ApplyEvent(
            new AlertRaisedIntegrationEvent(correlationId, alert.Id.ToString(), alertType.Value, severity.Value)
            {
                SessionId = treatmentSessionId.Value,
                TenantId = tenantId,
            });
        return alert;
    }

    public void Acknowledge(Ulid correlationId, string acknowledgedByUserId, string? tenantId)
    {
        if (LifecycleState != AlertLifecycleState.Active)
            throw new InvalidOperationException("Only an active alert can be acknowledged.");

        ArgumentException.ThrowIfNullOrWhiteSpace(acknowledgedByUserId);

        LifecycleState = AlertLifecycleState.Acknowledged;
        ApplyUpdateDateTime();
        ApplyEvent(
            new AlertAcknowledgedIntegrationEvent(correlationId, Id.ToString(), acknowledgedByUserId.Trim())
            {
                SessionId = TreatmentSessionId.Value,
                TenantId = tenantId,
            });
    }

    public void Escalate(Ulid correlationId, string escalationDetail, string? tenantId)
    {
        if (LifecycleState == AlertLifecycleState.Resolved)
            throw new InvalidOperationException("Cannot escalate a resolved alert.");
        if (LifecycleState == AlertLifecycleState.Escalated)
            throw new InvalidOperationException("Alert is already escalated.");

        ArgumentException.ThrowIfNullOrWhiteSpace(escalationDetail);

        LifecycleState = AlertLifecycleState.Escalated;
        ApplyUpdateDateTime();
        ApplyEvent(
            new AlertEscalatedIntegrationEvent(correlationId, Id.ToString(), escalationDetail.Trim())
            {
                SessionId = TreatmentSessionId.Value,
                TenantId = tenantId,
            });
    }

    public void Resolve(Ulid correlationId, string? resolutionNote, string? tenantId)
    {
        if (LifecycleState == AlertLifecycleState.Resolved)
            throw new InvalidOperationException("Alert is already resolved.");

        LifecycleState = AlertLifecycleState.Resolved;
        ApplyUpdateDateTime();
        string note = string.IsNullOrWhiteSpace(resolutionNote) ? string.Empty : resolutionNote.Trim();
        ApplyEvent(
            new AlertResolvedIntegrationEvent(correlationId, Id.ToString(), note)
            {
                SessionId = TreatmentSessionId.Value,
                TenantId = tenantId,
            });
    }

    private static string? TruncateDetail(string? detail)
    {
        if (string.IsNullOrWhiteSpace(detail))
            return null;
        string t = detail.Trim();
        return t.Length <= MaxDetailLength ? t : t[..MaxDetailLength];
    }
}
