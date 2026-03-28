using MeasurementValidation.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeasurementValidation.Infrastructure.Persistence;

internal sealed class ValidatedMeasurementEntityConfiguration : IEntityTypeConfiguration<ValidatedMeasurement>
{
    public void Configure(EntityTypeBuilder<ValidatedMeasurement> entity)
    {
        _ = entity.ToTable("validated_measurements");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.MeasurementId).HasMaxLength(ValidatedMeasurement.MaxMeasurementIdLength).IsRequired();
        _ = entity.Property(e => e.ValidationProfileId).HasMaxLength(ValidatedMeasurement.MaxValidationProfileIdLength).IsRequired();
        _ = entity.Property(e => e.Outcome).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.Reason).HasMaxLength(ValidatedMeasurement.MaxReasonLength);
        _ = entity.Property(e => e.RuleSetVersion).HasMaxLength(128).IsRequired();
        _ = entity.Property(e => e.EvaluatedAtUtc).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => new { e.MeasurementId, e.EvaluatedAtUtc });
    }
}
