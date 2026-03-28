using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.ValueObjects;

namespace ReplayRecovery.Infrastructure.Persistence;

internal sealed class RecoveryPlanExecutionEntityConfiguration : IEntityTypeConfiguration<RecoveryPlanExecution>
{
    public const int MaxOutcomeSummaryLength = 2000;

    public void Configure(EntityTypeBuilder<RecoveryPlanExecution> entity)
    {
        _ = entity.ToTable("recovery_plan_executions");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.PlanCode)
            .HasConversion(v => v.Value, v => new RecoveryPlanCode(v))
            .HasMaxLength(RecoveryPlanCode.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.State)
            .HasConversion(v => v.Value, v => RecoveryExecutionState.FromStored(v))
            .HasMaxLength(RecoveryExecutionState.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.OutcomeSummary).HasMaxLength(MaxOutcomeSummaryLength).IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
    }
}
