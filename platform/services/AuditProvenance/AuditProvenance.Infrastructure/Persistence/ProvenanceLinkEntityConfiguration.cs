using AuditProvenance.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuditProvenance.Infrastructure.Persistence;

internal sealed class ProvenanceLinkEntityConfiguration : IEntityTypeConfiguration<ProvenanceLink>
{
    public void Configure(EntityTypeBuilder<ProvenanceLink> entity)
    {
        _ = entity.ToTable("provenance_links");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.FromPlatformAuditFactId)
            .HasConversion(v => v.ToString(), v => Ulid.Parse(v))
            .IsRequired();
        _ = entity
            .Property(e => e.ToPlatformAuditFactId)
            .HasConversion(v => v.ToString(), v => Ulid.Parse(v))
            .IsRequired();
        _ = entity.Property(e => e.RelationType).HasMaxLength(ProvenanceLink.MaxRelationTypeLength).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();

        _ = entity
            .HasOne<PlatformAuditFact>()
            .WithMany()
            .HasForeignKey(e => e.FromPlatformAuditFactId)
            .OnDelete(DeleteBehavior.Restrict);

        _ = entity
            .HasOne<PlatformAuditFact>()
            .WithMany()
            .HasForeignKey(e => e.ToPlatformAuditFactId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
