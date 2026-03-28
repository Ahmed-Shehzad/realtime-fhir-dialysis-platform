using Microsoft.EntityFrameworkCore;

using BuildingBlocks.Persistence;

using DeviceRegistry.Domain;

namespace DeviceRegistry.Infrastructure.Persistence;

public sealed class DeviceRegistryDbContext : DbContext
{
    public DeviceRegistryDbContext(
        DbContextOptions<DeviceRegistryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<SecurityAuditLogEntry> SecurityAuditLogEntries => Set<SecurityAuditLogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.AddMassTransitTransactionalOutboxAndInbox();
        _ = modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeviceRegistryDbContext).Assembly);
    }
}
