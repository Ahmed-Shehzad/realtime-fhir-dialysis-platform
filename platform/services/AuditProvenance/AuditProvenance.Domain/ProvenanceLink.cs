using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

namespace AuditProvenance.Domain;

/// <summary>Directed link between two <see cref="PlatformAuditFact"/> records.</summary>
public sealed class ProvenanceLink : AggregateRoot
{
    public const int MaxRelationTypeLength = 128;

    private ProvenanceLink()
    {
    }

    public Ulid FromPlatformAuditFactId { get; private set; }

    public Ulid ToPlatformAuditFactId { get; private set; }

    public string RelationType { get; private set; } = null!;

    public static ProvenanceLink Create(
        Ulid correlationId,
        Ulid fromPlatformAuditFactId,
        Ulid toPlatformAuditFactId,
        string relationType,
        string? tenantId)
    {
        if (fromPlatformAuditFactId == toPlatformAuditFactId)
            throw new InvalidOperationException("Provenance link cannot reference the same fact as source and target.");

        string trimmedRelation = (relationType ?? string.Empty).Trim();
        if (trimmedRelation.Length == 0 || trimmedRelation.Length > MaxRelationTypeLength)
            throw new ArgumentException("RelationType is invalid.", nameof(relationType));

        var link = new ProvenanceLink
        {
            FromPlatformAuditFactId = fromPlatformAuditFactId,
            ToPlatformAuditFactId = toPlatformAuditFactId,
            RelationType = trimmedRelation,
        };

        static string? Tenant(string? t) =>
            string.IsNullOrWhiteSpace(t) ? null : t.Trim();

        link.ApplyCreatedDateTime();
        link.ApplyEvent(
            new ProvenanceRecordedIntegrationEvent(
                correlationId,
                link.Id.ToString(),
                fromPlatformAuditFactId.ToString(),
                toPlatformAuditFactId.ToString(),
                link.RelationType)
            {
                TenantId = Tenant(tenantId),
            });
        return link;
    }
}
