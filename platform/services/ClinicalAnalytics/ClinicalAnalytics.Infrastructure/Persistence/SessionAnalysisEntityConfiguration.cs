using BuildingBlocks.ValueObjects;

using ClinicalAnalytics.Domain;
using ClinicalAnalytics.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicalAnalytics.Infrastructure.Persistence;

internal sealed class SessionAnalysisEntityConfiguration : IEntityTypeConfiguration<SessionAnalysis>
{
    public const int MaxTreatmentSessionIdLength = 256;
    public const int MaxTrendSummaryLength = 2000;

    public void Configure(EntityTypeBuilder<SessionAnalysis> entity)
    {
        _ = entity.ToTable("session_analyses");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.TreatmentSessionId)
            .HasConversion(v => v.Value, v => new SessionId(v))
            .HasMaxLength(MaxTreatmentSessionIdLength)
            .IsRequired();
        _ = entity
            .Property(e => e.AnalyticalModelVersion)
            .HasConversion(v => v.Value, v => new ModelVersion(v))
            .HasMaxLength(ModelVersion.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.OverallConfidence).HasConversion(v => v.Percent, v => ConfidenceScore.FromPercent(v));
        _ = entity
            .Property(e => e.Interpretation)
            .HasConversion(v => v.Value, v => InterpretationStatus.FromStored(v))
            .HasMaxLength(InterpretationStatus.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.TrendSummary).HasMaxLength(MaxTrendSummaryLength).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => e.TreatmentSessionId);
        _ = entity
            .HasMany(e => e.DerivedMetrics)
            .WithOne()
            .HasForeignKey(e => e.SessionAnalysisId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
