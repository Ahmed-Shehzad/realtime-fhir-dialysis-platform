using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using ClinicalInteroperability.Domain;

namespace ClinicalInteroperability.Infrastructure.Persistence;

public sealed class ClinicalInteroperabilityDbContext : DbContext
{
    public ClinicalInteroperabilityDbContext(
        DbContextOptions<ClinicalInteroperabilityDbContext> options)
        : base(options)
    {
    }

    public DbSet<CanonicalObservationPublication> CanonicalObservationPublications => Set<CanonicalObservationPublication>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(ClinicalInteroperabilityDbContext).Assembly);
    }
}
