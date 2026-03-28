using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ClinicalAnalytics.Infrastructure.Persistence;

public sealed class ClinicalAnalyticsDbContextFactory : IDesignTimeDbContextFactory<ClinicalAnalyticsDbContext>
{
    public ClinicalAnalyticsDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ClinicalAnalyticsDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=clinical_analytics_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new ClinicalAnalyticsDbContext(optionsBuilder.Options);
    }
}
