using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using TreatmentSession.Domain;

namespace TreatmentSession.Infrastructure.Persistence;

public sealed class TreatmentSessionDbContext : DbContext
{
    public TreatmentSessionDbContext(
        DbContextOptions<TreatmentSessionDbContext> options)
        : base(options)
    {
    }

    public DbSet<DialysisSession> DialysisSessions => Set<DialysisSession>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(TreatmentSessionDbContext).Assembly);
    }
}
