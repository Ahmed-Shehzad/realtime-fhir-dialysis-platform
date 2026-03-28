using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using AuditProvenance.Domain;

namespace AuditProvenance.Infrastructure.Persistence;

public sealed class AuditProvenanceDbContext : DbContext
{
    public AuditProvenanceDbContext(
        DbContextOptions<AuditProvenanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<PlatformAuditFact> PlatformAuditFacts => Set<PlatformAuditFact>();

    public DbSet<ProvenanceLink> ProvenanceLinks => Set<ProvenanceLink>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuditProvenanceDbContext).Assembly);
    }
}
