using BuildingBlocks.ValueObjects;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.ValueObjects;

namespace WorkflowOrchestrator.Infrastructure.Persistence;

internal sealed class WorkflowInstanceEntityConfiguration : IEntityTypeConfiguration<WorkflowInstance>
{
    public const int MaxTreatmentSessionIdLength = 256;

    public void Configure(EntityTypeBuilder<WorkflowInstance> entity)
    {
        _ = entity.ToTable("workflow_instances");
        _ = entity.HasKey(e => e.Id);
        _ = entity.Property(e => e.Id).HasConversion(v => v.ToString(), v => Ulid.Parse(v));
        _ = entity
            .Property(e => e.Kind)
            .HasConversion(v => v.Value, v => WorkflowKind.FromStored(v))
            .HasMaxLength(WorkflowKind.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.State)
            .HasConversion(v => v.Value, v => WorkflowLifecycleState.FromStored(v))
            .HasMaxLength(WorkflowLifecycleState.MaxLength)
            .IsRequired();
        _ = entity
            .Property(e => e.TreatmentSessionId)
            .HasConversion(v => v.Value, v => new SessionId(v))
            .HasMaxLength(MaxTreatmentSessionIdLength)
            .IsRequired();
        _ = entity.Property(e => e.StepOrdinal).IsRequired();
        _ = entity
            .Property(e => e.CurrentStepName)
            .HasConversion(v => v.Value, v => new WorkflowStepName(v))
            .HasMaxLength(WorkflowStepName.MaxLength)
            .IsRequired();
        _ = entity.Property(e => e.CreatedAtUtc).IsRequired();
        _ = entity.Property(e => e.UpdatedAtUtc).IsRequired();
        _ = entity.HasIndex(e => e.TreatmentSessionId);
    }
}
