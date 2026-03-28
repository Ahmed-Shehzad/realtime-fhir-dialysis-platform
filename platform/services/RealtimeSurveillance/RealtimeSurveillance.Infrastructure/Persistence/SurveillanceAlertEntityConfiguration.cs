using BuildingBlocks.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Infrastructure.Persistence;

internal sealed class SurveillanceAlertEntityConfiguration : IEntityTypeConfiguration<SurveillanceAlert>
{
    public const int MaxTreatmentSessionIdLength = 256;

    public void Configure(EntityTypeBuilder<SurveillanceAlert> entity)
    {
        _ = entity.ToTable("surveillance_alerts");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.TreatmentSessionId)
            .HasConversion(v => v.Value, v => new SessionId(v))
            .HasMaxLength(MaxTreatmentSessionIdLength)
            .IsRequired();
        _ = entity
            .Property(e => e.AlertType)
            .HasConversion(v => v.Value, v => new AlertTypeCode(v))
            .HasMaxLength(AlertTypeCode.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.Severity)
            .HasConversion(v => v.Value, v => new AlertSeverityLevel(v))
            .HasMaxLength(AlertSeverityLevel.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.LifecycleState)
            .HasConversion(v => v.Value, v => AlertLifecycleState.FromStored(v))
            .HasMaxLength(AlertLifecycleState.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.Detail).HasMaxLength(SurveillanceAlert.MaxDetailLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => e.TreatmentSessionId);
    }
}
