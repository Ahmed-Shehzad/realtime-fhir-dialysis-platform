using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using WorkflowOrchestrator.Domain;

namespace WorkflowOrchestrator.Infrastructure.Persistence;

public sealed class WorkflowOrchestratorDbContext : DbContext
{
    public WorkflowOrchestratorDbContext(
        DbContextOptions<WorkflowOrchestratorDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkflowInstance> WorkflowInstances => Set<WorkflowInstance>();

    public DbSet<WorkflowOrchestratorSecurityAuditLogEntry> WorkflowOrchestratorSecurityAuditLogEntries =>
        Set<WorkflowOrchestratorSecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(WorkflowOrchestratorDbContext).Assembly);
    }
}
