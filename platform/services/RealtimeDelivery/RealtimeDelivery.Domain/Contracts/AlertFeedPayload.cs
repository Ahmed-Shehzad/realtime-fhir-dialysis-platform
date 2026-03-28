namespace RealtimeDelivery.Domain.Contracts;

public sealed record AlertFeedPayload(
    string EventType,
    string? TreatmentSessionId,
    string AlertId,
    string Severity,
    string LifecycleState,
    DateTimeOffset OccurredAtUtc);
