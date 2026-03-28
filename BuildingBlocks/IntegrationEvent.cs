using BuildingBlocks.Abstractions;

using RealtimePlatform.Messaging;

namespace BuildingBlocks;

/// <summary>
/// Base type for integration events with standard metadata aligned to the platform integration event envelope (catalog §1).
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    protected IntegrationEvent(Ulid correlationId)
    {
        CorrelationId = correlationId;
        EventId = Ulid.NewUlid();
        OccurredOn = DateTimeOffset.UtcNow;
    }

    public Ulid EventId { get; init; }

    public DateTimeOffset OccurredOn { get; init; }

    public Ulid CorrelationId { get; }

    /// <summary>Contract version for consumers (envelope <c>eventVersion</c>).</summary>
    public int EventVersion { get; init; } = 1;

    /// <summary>Optional parent event (envelope <c>causationId</c>).</summary>
    public Ulid? CausationId { get; init; }

    public Ulid? WorkflowId { get; init; }

    public Ulid? SagaId { get; init; }

    public string? FacilityId { get; init; }

    public string? SessionId { get; init; }

    public string? PatientId { get; init; }

    /// <summary>Optional routing hint; maps to envelope <c>deviceId</c> when payload does not carry it.</summary>
    public string? RoutingDeviceId { get; init; }

    public string? PartitionKey { get; init; }

    /// <summary>Optional multi-tenant routing (envelope extension alongside <c>facilityId</c>; keep aligned with catalog evolution).</summary>
    public string? TenantId { get; init; }

    Ulid? ICorrelatedMessage.CorrelationId => CorrelationId;
}
