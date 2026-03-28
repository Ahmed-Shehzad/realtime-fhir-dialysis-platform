using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using SignalConditioning.Domain;

namespace SignalConditioning.Infrastructure.Persistence;

public sealed class SignalConditioningDbContext : DbContext
{
    public SignalConditioningDbContext(
        DbContextOptions<SignalConditioningDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConditioningResult> ConditioningResults => Set<ConditioningResult>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(SignalConditioningDbContext).Assembly);
    }
}
