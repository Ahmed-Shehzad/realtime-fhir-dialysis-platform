using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MeasurementAcquisition.Infrastructure.Persistence;

/// <summary>
/// Design-time factory for EF migrations. Uses <c>REALTIME_PLATFORM_EF_CONNECTION</c> or a local default.
/// </summary>
public sealed class MeasurementAcquisitionDbContextFactory : IDesignTimeDbContextFactory<MeasurementAcquisitionDbContext>
{
    /// <inheritdoc />
    public MeasurementAcquisitionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<MeasurementAcquisitionDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=measurement_acquisition_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new MeasurementAcquisitionDbContext(optionsBuilder.Options);
    }
}
