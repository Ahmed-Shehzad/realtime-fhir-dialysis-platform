using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.ValueObjects;

namespace ReplayRecovery.Infrastructure.Persistence;

internal sealed class ReplayJobEntityConfiguration : IEntityTypeConfiguration<ReplayJob>
{
    public void Configure(EntityTypeBuilder<ReplayJob> entity)
    {
        _ = entity.ToTable("replay_jobs");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.Mode)
            .HasConversion(v => v.Value, v => ReplayMode.FromStored(v))
            .HasMaxLength(ReplayMode.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.State)
            .HasConversion(v => v.Value, v => ReplayJobState.FromStored(v))
            .HasMaxLength(ReplayJobState.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.ProjectionSet)
            .HasColumnName("ProjectionSetName")
            .HasConversion(v => v.Value, v => new ProjectionSetName(v))
            .HasMaxLength(ProjectionSetName.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.CheckpointSequence).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
