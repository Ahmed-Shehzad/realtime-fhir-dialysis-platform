using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.ValueObjects;

namespace AdministrationConfiguration.Infrastructure.Persistence;

internal sealed class ThresholdProfileEntityConfiguration : IEntityTypeConfiguration<ThresholdProfile>
{
    public void Configure(EntityTypeBuilder<ThresholdProfile> entity)
    {
        _ = entity.ToTable("threshold_profiles");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.ProfileCode)
            .HasConversion(v => v.Value, v => new ThresholdProfileCode(v))
            .HasMaxLength(ThresholdProfileCode.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.Payload)
            .HasColumnName("PayloadJson")
            .HasConversion(v => v.Json, v => new ThresholdProfilePayload(v))
            .IsRequired();
        _ = entity.Property(e => e.ProfileRevision).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.DeletedAtUtc);
        _ = entity.Property(e => e.IsDeleted).IsRequired();
    }
}
