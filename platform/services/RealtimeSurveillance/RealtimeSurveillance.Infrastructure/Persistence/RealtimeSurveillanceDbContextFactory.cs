using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace RealtimeSurveillance.Infrastructure.Persistence;

public sealed class RealtimeSurveillanceDbContextFactory : IDesignTimeDbContextFactory<RealtimeSurveillanceDbContext>
{
    public RealtimeSurveillanceDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RealtimeSurveillanceDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=realtime_surveillance_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new RealtimeSurveillanceDbContext(optionsBuilder.Options);
    }
}
