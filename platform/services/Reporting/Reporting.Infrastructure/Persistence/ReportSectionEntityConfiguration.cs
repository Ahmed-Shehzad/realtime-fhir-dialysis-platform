using Reporting.Domain;
using Reporting.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reporting.Infrastructure.Persistence;

internal sealed class ReportSectionEntityConfiguration : IEntityTypeConfiguration<ReportSection>
{
    public const int MaxBodyLength = 8000;

    public void Configure(EntityTypeBuilder<ReportSection> entity)
    {
        _ = entity.ToTable("report_sections");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.SessionReportId).HasConversion(v => v.ToString(), v => Ulid.Parse(v)).IsRequired();
        _ = entity
            .Property(e => e.Heading)
            .HasConversion(v => v.Value, v => new SectionHeading(v))
            .HasMaxLength(SectionHeading.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.Body).HasMaxLength(MaxBodyLength).IsRequired();
        _ = entity.HasIndex(e => e.SessionReportId);
    }
}
