using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace AuditProvenance.Domain;

/// <summary>Input for <see cref="PlatformAuditFact.Record"/> (keeps factory arity within analyzer limits).</summary>
public sealed record PlatformAuditFactPayload
{
    public required DateTimeOffset OccurredAtUtc { get; init; }

    public required string EventType { get; init; }

    public required string Summary { get; init; }

    public string? DetailJson { get; init; }

    public string? CorrelationIdString { get; init; }

    public string? CausationIdString { get; init; }

    public string? TenantId { get; init; }

    public string? ActorId { get; init; }

    public required string SourceSystem { get; init; }

    public string? RelatedResourceType { get; init; }

    public string? RelatedResourceId { get; init; }

    public string? SessionId { get; init; }

    public string? PatientId { get; init; }
}

/// <summary>Append-only platform audit fact with optional correlation to sessions/patients.</summary>
public sealed class PlatformAuditFact : AggregateRoot
{
    public const int MaxEventTypeLength = 256;

    public const int MaxSummaryLength = 4000;

    public const int MaxSourceSystemLength = 128;

    public const int MaxRelatedResourceTypeLength = 128;

    public const int MaxRelatedResourceIdLength = 512;

    public const int MaxOptionalIdLength = 200;

    public const int MaxActorLength = 256;

    public const int MaxRoutingIdLength = 128;

    private PlatformAuditFact()
    {
    }

    public DateTimeOffset OccurredAtUtc { get; private set; }

    public string EventType { get; private set; } = null!;

    public string Summary { get; private set; } = null!;

    public string? DetailJson { get; private set; }

    public string? CorrelationId { get; private set; }

    public string? CausationId { get; private set; }

    public string? TenantId { get; private set; }

    public string? ActorId { get; private set; }

    public string SourceSystem { get; private set; } = null!;

    public string? RelatedResourceType { get; private set; }

    public string? RelatedResourceId { get; private set; }

    public string? SessionId { get; private set; }

    public string? PatientId { get; private set; }

    public static PlatformAuditFact Record(Ulid correlationId, PlatformAuditFactPayload payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        string trimmedType = (payload.EventType ?? string.Empty).Trim();
        if (trimmedType.Length == 0 || trimmedType.Length > MaxEventTypeLength)
            throw new ArgumentException("EventType is invalid.", nameof(payload));

        string trimmedSummary = (payload.Summary ?? string.Empty).Trim();
        if (trimmedSummary.Length == 0 || trimmedSummary.Length > MaxSummaryLength)
            throw new ArgumentException("Summary is invalid.", nameof(payload));

        string trimmedSource = (payload.SourceSystem ?? string.Empty).Trim();
        if (trimmedSource.Length == 0 || trimmedSource.Length > MaxSourceSystemLength)
            throw new ArgumentException("SourceSystem is invalid.", nameof(payload));

        string? ClampOptional(string? value, int max, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            string t = value.Trim();
            return t.Length <= max
                ? t
                : throw new ArgumentException($"{fieldName} exceeds max length {max}.");
        }

        string? json = string.IsNullOrWhiteSpace(payload.DetailJson) ? null : payload.DetailJson.Trim();

        var fact = new PlatformAuditFact
        {
            OccurredAtUtc = payload.OccurredAtUtc,
            EventType = trimmedType,
            Summary = trimmedSummary,
            DetailJson = json,
            CorrelationId = ClampOptional(payload.CorrelationIdString, MaxOptionalIdLength, nameof(PlatformAuditFactPayload.CorrelationIdString)),
            CausationId = ClampOptional(payload.CausationIdString, MaxOptionalIdLength, nameof(PlatformAuditFactPayload.CausationIdString)),
            TenantId = ClampOptional(payload.TenantId, MaxRoutingIdLength, nameof(PlatformAuditFactPayload.TenantId)),
            ActorId = ClampOptional(payload.ActorId, MaxActorLength, nameof(PlatformAuditFactPayload.ActorId)),
            SourceSystem = trimmedSource,
            RelatedResourceType = ClampOptional(
                payload.RelatedResourceType,
                MaxRelatedResourceTypeLength,
                nameof(PlatformAuditFactPayload.RelatedResourceType)),
            RelatedResourceId = ClampOptional(
                payload.RelatedResourceId,
                MaxRelatedResourceIdLength,
                nameof(PlatformAuditFactPayload.RelatedResourceId)),
            SessionId = ClampOptional(payload.SessionId, MaxRoutingIdLength, nameof(PlatformAuditFactPayload.SessionId)),
            PatientId = ClampOptional(payload.PatientId, MaxRoutingIdLength, nameof(PlatformAuditFactPayload.PatientId)),
        };

        fact.ApplyCreatedDateTime();
        fact.ApplyEvent(
            new CriticalAuditEventRecordedIntegrationEvent(correlationId, fact.Id.ToString(), fact.EventType)
            {
                TenantId = fact.TenantId,
                SessionId = fact.SessionId,
                PatientId = fact.PatientId,
            });
        return fact;
    }
}
