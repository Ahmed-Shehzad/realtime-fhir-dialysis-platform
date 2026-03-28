using BuildingBlocks.ValueObjects;

using Reporting.Domain;
using Reporting.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Reporting.Infrastructure.Persistence;

internal sealed class SessionReportEntityConfiguration : IEntityTypeConfiguration<SessionReport>
{
    public const int MaxTreatmentSessionIdLength = 256;

    public void Configure(EntityTypeBuilder<SessionReport> entity)
    {
        _ = entity.ToTable("session_reports");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.TreatmentSessionId)
            .HasConversion(v => v.Value, v => new SessionId(v))
            .HasMaxLength(MaxTreatmentSessionIdLength)
            .IsRequired();
        _ = entity
            .Property(e => e.Status)
            .HasConversion(v => v.Value, v => ReportStatus.FromStored(v))
            .HasMaxLength(ReportStatus.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.NarrativeVersion)
            .HasConversion(v => v.Value, v => new NarrativeVersion(v))
            .HasMaxLength(NarrativeVersion.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => e.TreatmentSessionId);
        _ = entity
            .HasMany(e => e.Sections)
            .WithOne()
            .HasForeignKey(e => e.SessionReportId)
            .OnDelete(DeleteBehavior.Cascade);
        _ = entity
            .HasMany(e => e.EvidenceItems)
            .WithOne()
            .HasForeignKey(e => e.SessionReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
