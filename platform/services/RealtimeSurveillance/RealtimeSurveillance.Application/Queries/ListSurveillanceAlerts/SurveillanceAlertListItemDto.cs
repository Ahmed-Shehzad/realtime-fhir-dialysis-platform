namespace RealtimeSurveillance.Application.Queries.ListSurveillanceAlerts;

public sealed record SurveillanceAlertListItemDto(
    string Id,
    string TreatmentSessionId,
    string AlertType,
    string Severity,
    string LifecycleState,
    DateTime CreatedAtUtc);
