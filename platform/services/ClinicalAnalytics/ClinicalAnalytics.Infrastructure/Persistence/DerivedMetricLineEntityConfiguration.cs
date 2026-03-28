using ClinicalAnalytics.Domain;
using ClinicalAnalytics.Domain.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ClinicalAnalytics.Infrastructure.Persistence;

internal sealed class DerivedMetricLineEntityConfiguration : IEntityTypeConfiguration<DerivedMetricLine>
{
    public const int MaxValueLength = 128;
    public const int MaxUnitLength = 32;

    public void Configure(EntityTypeBuilder<DerivedMetricLine> entity)
    {
        _ = entity.ToTable("session_analysis_derived_metrics");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.SessionAnalysisId).HasConversion(v => v.ToString(), v => Ulid.Parse(v)).IsRequired();
        _ = entity
            .Property(e => e.Code)
            .HasConversion(v => v.Value, v => new MetricCode(v))
            .HasMaxLength(MetricCode.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.Value).HasMaxLength(MaxValueLength).IsRequired();
        _ = entity.Property(e => e.Unit).HasMaxLength(MaxUnitLength);
        _ = entity.HasIndex(e => e.SessionAnalysisId);
    }
}
