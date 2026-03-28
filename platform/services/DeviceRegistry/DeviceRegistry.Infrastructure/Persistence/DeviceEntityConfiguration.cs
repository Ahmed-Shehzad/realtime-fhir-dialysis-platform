using BuildingBlocks.ValueObjects;

using DeviceRegistry.Domain;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeviceRegistry.Infrastructure.Persistence;

internal sealed class DeviceEntityConfiguration : IEntityTypeConfiguration<Device>
{
    public void Configure(EntityTypeBuilder<Device> entity)
    {
        _ = entity.ToTable("devices");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity.HasIndex(e => e.DeviceId).IsUnique();
        _ = entity.Property(e => e.DeviceId)
            .HasConversion(v => v.Value, v => new DeviceId(v))
            .HasMaxLength(128)
            .IsRequired();
        _ = entity.Property(e => e.TrustState)
            .HasConversion(v => v.Value, v => TrustState.From(v))
            .HasMaxLength(64)
            .IsRequired();
        _ = entity.Property(e => e.Manufacturer).HasMaxLength(Device.MaxManufacturerLength);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
