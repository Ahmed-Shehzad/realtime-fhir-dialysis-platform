using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using MeasurementAcquisition.Domain;

namespace MeasurementAcquisition.Infrastructure.Persistence;

public sealed class MeasurementAcquisitionDbContext : DbContext
{
    public MeasurementAcquisitionDbContext(
        DbContextOptions<MeasurementAcquisitionDbContext> options)
        : base(options)
    {
    }

    public DbSet<RawMeasurementEnvelope> RawMeasurementEnvelopes => Set<RawMeasurementEnvelope>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeasurementAcquisitionDbContext).Assembly);
    }
}
