using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RealtimePlatform.Persistence;

/// <summary>
/// Design-time factory for EF Core CLI. Set environment variable <c>REALTIME_PLATFORM_EF_CONNECTION</c>
/// to a PostgreSQL connection string, or the default local development connection is used.
/// </summary>
public sealed class SchemaOnlyDbContextFactory : IDesignTimeDbContextFactory<SchemaOnlyDbContext>
{
    /// <inheritdoc />
    public SchemaOnlyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SchemaOnlyDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=realtimeplatform_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new SchemaOnlyDbContext(optionsBuilder.Options);
    }
}
