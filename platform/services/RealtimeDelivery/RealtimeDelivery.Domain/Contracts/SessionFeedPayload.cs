namespace RealtimeDelivery.Domain.Contracts;

public sealed record SessionFeedPayload(
    string EventType,
    string TreatmentSessionId,
    string Summary,
    DateTimeOffset OccurredAtUtc);
