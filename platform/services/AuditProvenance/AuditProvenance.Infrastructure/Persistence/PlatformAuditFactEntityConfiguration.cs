using AuditProvenance.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditProvenance.Infrastructure.Persistence;

internal sealed class PlatformAuditFactEntityConfiguration : IEntityTypeConfiguration<PlatformAuditFact>
{
    public void Configure(EntityTypeBuilder<PlatformAuditFact> entity)
    {
        _ = entity.ToTable("platform_audit_facts");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.OccurredAtUtc).IsRequired();
        _ = entity.Property(e => e.EventType).HasMaxLength(PlatformAuditFact.MaxEventTypeLength).IsRequired();
        _ = entity.Property(e => e.Summary).HasMaxLength(PlatformAuditFact.MaxSummaryLength).IsRequired();
        _ = entity.Property(e => e.DetailJson);
        _ = entity.Property(e => e.CorrelationId).HasMaxLength(PlatformAuditFact.MaxOptionalIdLength);
        _ = entity.Property(e => e.CausationId).HasMaxLength(PlatformAuditFact.MaxOptionalIdLength);
        _ = entity.Property(e => e.TenantId).HasMaxLength(PlatformAuditFact.MaxRoutingIdLength);
        _ = entity.Property(e => e.ActorId).HasMaxLength(PlatformAuditFact.MaxActorLength);
        _ = entity.Property(e => e.SourceSystem).HasMaxLength(PlatformAuditFact.MaxSourceSystemLength).IsRequired();
        _ = entity.Property(e => e.RelatedResourceType).HasMaxLength(PlatformAuditFact.MaxRelatedResourceTypeLength);
        _ = entity.Property(e => e.RelatedResourceId).HasMaxLength(PlatformAuditFact.MaxRelatedResourceIdLength);
        _ = entity.Property(e => e.SessionId).HasMaxLength(PlatformAuditFact.MaxRoutingIdLength);
        _ = entity.Property(e => e.PatientId).HasMaxLength(PlatformAuditFact.MaxRoutingIdLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => e.OccurredAtUtc);
    }
}
