namespace RealtimeSurveillance.Application.Queries.GetSurveillanceAlertById;

public sealed record SurveillanceAlertReadDto(
    string Id,
    string TreatmentSessionId,
    string AlertType,
    string Severity,
    string LifecycleState,
    string? Detail,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
