using SignalConditioning.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SignalConditioning.Infrastructure.Persistence;

internal sealed class ConditioningResultEntityConfiguration : IEntityTypeConfiguration<ConditioningResult>
{
    public void Configure(EntityTypeBuilder<ConditioningResult> entity)
    {
        _ = entity.ToTable("conditioning_results");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.MeasurementId).HasMaxLength(ConditioningResult.MaxMeasurementIdLength).IsRequired();
        _ = entity.Property(e => e.ChannelId).HasMaxLength(ConditioningResult.MaxChannelIdLength).IsRequired();
        _ = entity.Property(e => e.IsDropout).IsRequired();
        _ = entity.Property(e => e.DriftDetected).IsRequired();
        _ = entity.Property(e => e.QualityScorePercent).IsRequired();
        _ = entity.Property(e => e.ConditioningMethodVersion).HasMaxLength(128).IsRequired();
        _ = entity.Property(e => e.ConditionedSignalKind).HasMaxLength(128);
        _ = entity.Property(e => e.EvaluatedAtUtc).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => new { e.MeasurementId, e.EvaluatedAtUtc });
    }
}
