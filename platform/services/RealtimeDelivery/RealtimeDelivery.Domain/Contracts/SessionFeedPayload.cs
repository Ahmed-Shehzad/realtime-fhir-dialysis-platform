namespace RealtimeDelivery.Domain.Contracts;

public sealed record SessionFeedPayload(
    string EventType,
    string TreatmentSessionId,
    string Summary,
    DateTimeOffset OccurredAtUtc,
    IReadOnlyDictionary<string, double>? VitalsByChannel = null,
    string? PatientDisplayLabel = null,
    string? SessionStateHint = null,
    string? LinkedDeviceIdHint = null);
