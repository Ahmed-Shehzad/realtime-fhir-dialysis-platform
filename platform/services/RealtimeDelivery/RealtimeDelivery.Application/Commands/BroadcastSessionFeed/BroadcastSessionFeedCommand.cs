using Intercessor.Abstractions;

namespace RealtimeDelivery.Application.Commands.BroadcastSessionFeed;

public sealed record BroadcastSessionFeedCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string EventType,
    string Summary,
    DateTimeOffset OccurredAtUtc,
    string? AuthenticatedUserId = null,
    IReadOnlyDictionary<string, double>? VitalsByChannel = null,
    string? PatientDisplayLabel = null,
    string? SessionStateHint = null,
    string? LinkedDeviceIdHint = null) : ICommand;
