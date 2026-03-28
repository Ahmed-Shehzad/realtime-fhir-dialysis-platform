using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using ReplayRecovery.Domain;

namespace ReplayRecovery.Infrastructure.Persistence;

public sealed class ReplayRecoveryDbContext : DbContext
{
    public ReplayRecoveryDbContext(
        DbContextOptions<ReplayRecoveryDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReplayJob> ReplayJobs => Set<ReplayJob>();

    public DbSet<RecoveryPlanExecution> RecoveryPlanExecutions => Set<RecoveryPlanExecution>();

    public DbSet<ReplayRecoverySecurityAuditLogEntry> ReplayRecoverySecurityAuditLogEntries =>
        Set<ReplayRecoverySecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReplayRecoveryDbContext).Assembly);
    }
}
