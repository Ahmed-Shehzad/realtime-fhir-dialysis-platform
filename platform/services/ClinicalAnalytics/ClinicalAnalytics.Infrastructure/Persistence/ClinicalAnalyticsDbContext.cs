using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using ClinicalAnalytics.Domain;

namespace ClinicalAnalytics.Infrastructure.Persistence;

public sealed class ClinicalAnalyticsDbContext : DbContext
{
    public ClinicalAnalyticsDbContext(
        DbContextOptions<ClinicalAnalyticsDbContext> options)
        : base(options)
    {
    }

    public DbSet<SessionAnalysis> SessionAnalyses => Set<SessionAnalysis>();

    public DbSet<ClinicalAnalyticsSecurityAuditLogEntry> ClinicalAnalyticsSecurityAuditLogEntries =>
        Set<ClinicalAnalyticsSecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicalAnalyticsDbContext).Assembly);
    }
}
