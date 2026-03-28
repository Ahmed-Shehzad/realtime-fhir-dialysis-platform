namespace RealtimePlatform.MassTransit;

/// <summary>Broker DTO for durable Intercessor command dispatch (not an application <c>IRequest</c>).</summary>
public sealed record IntercessorCommandEnvelope
{
    /// <summary>Result of <see cref="System.Type.AssemblyQualifiedName"/> for the command type.</summary>
    public required string RequestTypeAssemblyQualifiedName { get; init; }

    /// <summary>JSON document for the command, serialized with the concrete runtime type.</summary>
    public required string PayloadJson { get; init; }

    /// <summary>Optional correlation / trace id.</summary>
    public string? CorrelationId { get; init; }

    /// <summary>Optional tenant id (C5 multi-tenancy); handlers may read from a scoped accessor if wired.</summary>
    public string? TenantId { get; init; }
}
