using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using RealtimeSurveillance.Domain;

namespace RealtimeSurveillance.Infrastructure.Persistence;

public sealed class RealtimeSurveillanceDbContext : DbContext
{
    public RealtimeSurveillanceDbContext(
        DbContextOptions<RealtimeSurveillanceDbContext> options)
        : base(options)
    {
    }

    public DbSet<SurveillanceAlert> SurveillanceAlerts => Set<SurveillanceAlert>();

    public DbSet<SessionRiskSnapshot> SessionRiskSnapshots => Set<SessionRiskSnapshot>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(RealtimeSurveillanceDbContext).Assembly);
    }
}
