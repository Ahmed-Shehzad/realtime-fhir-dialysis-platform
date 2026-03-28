using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DeviceRegistry.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF migrations. Uses <c>REALTIME_PLATFORM_EF_CONNECTION</c> or a local default.
/// </summary>
public sealed class DeviceRegistryDbContextFactory : IDesignTimeDbContextFactory<DeviceRegistryDbContext>
{
    /// <inheritdoc />
    public DeviceRegistryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DeviceRegistryDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=device_registry_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new DeviceRegistryDbContext(optionsBuilder.Options);
    }
}
