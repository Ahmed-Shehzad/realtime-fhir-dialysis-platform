using BuildingBlocks.ValueObjects;

using MeasurementAcquisition.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MeasurementAcquisition.Infrastructure.Persistence;

internal sealed class RawMeasurementEnvelopeEntityConfiguration : IEntityTypeConfiguration<RawMeasurementEnvelope>
{
    public void Configure(EntityTypeBuilder<RawMeasurementEnvelope> entity)
    {
        _ = entity.ToTable("raw_measurement_envelopes");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.Property(e => e.DeviceId)
            .HasConversion(v => v.Value, v => new DeviceId(v))
            .HasMaxLength(128)
            .IsRequired();
        _ = entity.Property(e => e.Channel).HasMaxLength(RawMeasurementEnvelope.MaxChannelLength).IsRequired();
        _ = entity.Property(e => e.MeasurementType)
            .HasMaxLength(RawMeasurementEnvelope.MaxMeasurementTypeLength)
            .IsRequired();
        _ = entity.Property(e => e.SchemaVersion)
            .HasMaxLength(RawMeasurementEnvelope.MaxSchemaVersionLength)
            .IsRequired();
        _ = entity.Property(e => e.PayloadHash).HasMaxLength(64).IsRequired();
        _ = entity.Property(e => e.RawPayloadJson).HasColumnType("text").IsRequired();
        _ = entity.Property(e => e.Status).HasConversion<int>().IsRequired();
        _ = entity.Property(e => e.RejectionReason).HasMaxLength(2048);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => new { e.DeviceId, e.Channel, e.CreatedAtUtc });
    }
}
