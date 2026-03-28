using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicalInteroperability.Infrastructure.Persistence;

public sealed class ClinicalInteroperabilityDbContextFactory : IDesignTimeDbContextFactory<ClinicalInteroperabilityDbContext>
{
    public ClinicalInteroperabilityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClinicalInteroperabilityDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=clinical_interoperability_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new ClinicalInteroperabilityDbContext(optionsBuilder.Options);
    }
}
