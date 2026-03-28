using Intercessor.Abstractions;

namespace RealtimeDelivery.Application.Commands.BroadcastAlertFeed;

public sealed record BroadcastAlertFeedCommand(
    Ulid CorrelationId,
    string EventType,
    string? TreatmentSessionId,
    string AlertId,
    string Severity,
    string LifecycleState,
    DateTimeOffset OccurredAtUtc,
    string? AuthenticatedUserId = null) : ICommand;
