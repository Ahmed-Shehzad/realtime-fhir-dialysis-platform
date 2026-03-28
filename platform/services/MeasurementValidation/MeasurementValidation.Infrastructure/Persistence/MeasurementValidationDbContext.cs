using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using MeasurementValidation.Domain;

namespace MeasurementValidation.Infrastructure.Persistence;

public sealed class MeasurementValidationDbContext : DbContext
{
    public MeasurementValidationDbContext(
        DbContextOptions<MeasurementValidationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ValidatedMeasurement> ValidatedMeasurements => Set<ValidatedMeasurement>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(MeasurementValidationDbContext).Assembly);
    }
}
