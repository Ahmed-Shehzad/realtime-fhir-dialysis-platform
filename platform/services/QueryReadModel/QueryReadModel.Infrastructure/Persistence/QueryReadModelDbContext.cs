using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using QueryReadModel.Domain;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class QueryReadModelDbContext : DbContext
{
    public QueryReadModelDbContext(
        DbContextOptions<QueryReadModelDbContext> options)
        : base(options)
    {
    }

    public DbSet<SessionOverviewProjection> SessionOverviewProjections => Set<SessionOverviewProjection>();

    public DbSet<AlertProjection> AlertProjections => Set<AlertProjection>();

    public DbSet<ReadModelRebuildRecord> ReadModelRebuildRecords => Set<ReadModelRebuildRecord>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(QueryReadModelDbContext).Assembly);
    }
}
