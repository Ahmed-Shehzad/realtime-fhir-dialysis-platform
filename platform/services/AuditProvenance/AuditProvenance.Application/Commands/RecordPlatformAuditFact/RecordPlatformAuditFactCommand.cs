using Intercessor.Abstractions;

namespace AuditProvenance.Application.Commands.RecordPlatformAuditFact;

public sealed record RecordPlatformAuditFactCommand(
    Ulid CorrelationId,
    DateTimeOffset? OccurredAtUtc,
    string EventType,
    string Summary,
    string? DetailJson,
    string? CorrelationIdString,
    string? CausationIdString,
    string? ActorId,
    string SourceSystem,
    string? RelatedResourceType,
    string? RelatedResourceId,
    string? SessionId,
    string? PatientId,
    string? AuthenticatedUserId = null) : ICommand<RecordPlatformAuditFactResult>;
