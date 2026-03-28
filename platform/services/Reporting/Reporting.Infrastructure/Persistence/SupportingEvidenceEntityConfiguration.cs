using Reporting.Domain;
using Reporting.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reporting.Infrastructure.Persistence;

internal sealed class SupportingEvidenceEntityConfiguration : IEntityTypeConfiguration<SupportingEvidence>
{
    public void Configure(EntityTypeBuilder<SupportingEvidence> entity)
    {
        _ = entity.ToTable("session_report_supporting_evidence");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.SessionReportId).HasConversion(v => v.ToString(), v => Ulid.Parse(v)).IsRequired();
        _ = entity
            .Property(e => e.Kind)
            .HasConversion(v => v.Value, v => EvidenceKind.FromStored(v))
            .HasMaxLength(EvidenceKind.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.Locator)
            .HasConversion(v => v.Value, v => new EvidenceLocator(v))
            .HasMaxLength(EvidenceLocator.MaxLength)
            .IsRequired();
        _ = entity.HasIndex(e => e.SessionReportId);
    }
}
