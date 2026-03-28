namespace ClinicalInteroperability.Infrastructure.Persistence;

public sealed class SecurityAuditLogEntry
{
    public Guid Id { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    public int Action { get; set; }

    public required string ResourceType { get; set; }

    public string? ResourceId { get; set; }

    public string? UserId { get; set; }

    public int Outcome { get; set; }

    public string? Description { get; set; }

    public string? TenantId { get; set; }

    public string? CorrelationId { get; set; }
}
