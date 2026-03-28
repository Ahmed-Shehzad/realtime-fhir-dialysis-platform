using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using Reporting.Domain;

namespace Reporting.Infrastructure.Persistence;

public sealed class ReportingDbContext : DbContext
{
    public ReportingDbContext(
        DbContextOptions<ReportingDbContext> options)
        : base(options)
    {
    }

    public DbSet<SessionReport> SessionReports => Set<SessionReport>();

    public DbSet<ReportingSecurityAuditLogEntry> ReportingSecurityAuditLogEntries =>
        Set<ReportingSecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ReportingDbContext).Assembly);
    }
}
