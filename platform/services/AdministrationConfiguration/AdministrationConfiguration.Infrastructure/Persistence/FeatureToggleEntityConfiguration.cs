using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.ValueObjects;

namespace AdministrationConfiguration.Infrastructure.Persistence;

internal sealed class FeatureToggleEntityConfiguration : IEntityTypeConfiguration<FeatureToggle>
{
    public void Configure(EntityTypeBuilder<FeatureToggle> entity)
    {
        _ = entity.ToTable("feature_toggles");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.FeatureKey)
            .HasConversion(v => v.Value, v => new FeatureFlagKey(v))
            .HasMaxLength(FeatureFlagKey.MaxLength)
            .IsRequired();
        _ = entity.HasIndex(e => e.FeatureKey).IsUnique();
        _ = entity.Property(e => e.IsEnabled).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.Property(e => e.DeletedAtUtc);
        _ = entity.Property(e => e.IsDeleted).IsRequired();
    }
}
