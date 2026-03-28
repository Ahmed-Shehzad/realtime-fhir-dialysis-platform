using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TerminologyConformance.Infrastructure.Persistence;

public sealed class TerminologyConformanceDbContextFactory : IDesignTimeDbContextFactory<TerminologyConformanceDbContext>
{
    public TerminologyConformanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TerminologyConformanceDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=terminology_conformance_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new TerminologyConformanceDbContext(optionsBuilder.Options);
    }
}
