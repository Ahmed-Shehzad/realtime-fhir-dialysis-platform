using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeasurementValidation.Infrastructure.Persistence;

public sealed class MeasurementValidationDbContextFactory : IDesignTimeDbContextFactory<MeasurementValidationDbContext>
{
    public MeasurementValidationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MeasurementValidationDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=measurement_validation_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new MeasurementValidationDbContext(optionsBuilder.Options);
    }
}
