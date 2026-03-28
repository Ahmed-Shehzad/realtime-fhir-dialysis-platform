using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using TerminologyConformance.Domain;

namespace TerminologyConformance.Infrastructure.Persistence;

public sealed class TerminologyConformanceDbContext : DbContext
{
    public TerminologyConformanceDbContext(
        DbContextOptions<TerminologyConformanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<ConformanceAssessment> ConformanceAssessments => Set<ConformanceAssessment>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(TerminologyConformanceDbContext).Assembly);
    }
}
