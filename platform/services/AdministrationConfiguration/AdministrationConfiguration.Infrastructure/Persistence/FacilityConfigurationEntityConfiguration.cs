using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.ValueObjects;

namespace AdministrationConfiguration.Infrastructure.Persistence;

internal sealed class FacilityConfigurationEntityConfiguration : IEntityTypeConfiguration<FacilityConfiguration>
{
    public void Configure(EntityTypeBuilder<FacilityConfiguration> entity)
    {
        _ = entity.ToTable("facility_configurations");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.FacilityId)
            .HasConversion(v => v.Value, v => new FacilityId(v))
            .HasMaxLength(FacilityId.MaxLength)
            .IsRequired();
        _ = entity.HasIndex(e => e.FacilityId).IsUnique();
        _ = entity
            .Property(e => e.Configuration)
            .HasColumnName("ConfigurationJson")
            .HasConversion(v => v.Json, v => new ConfigurationPayload(v))
            .IsRequired();
        _ = entity.Property(e => e.ConfigurationRevision).IsRequired();
        _ = entity.Property(e => e.EffectiveFromUtc);
        _ = entity.Property(e => e.EffectiveToUtc);
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.DeletedAtUtc);
        _ = entity.Property(e => e.IsDeleted).IsRequired();
    }
}
