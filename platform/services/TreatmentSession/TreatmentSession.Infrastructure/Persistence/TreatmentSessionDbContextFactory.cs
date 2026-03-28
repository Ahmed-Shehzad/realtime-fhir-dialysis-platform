using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TreatmentSession.Infrastructure.Persistence;

public sealed class TreatmentSessionDbContextFactory : IDesignTimeDbContextFactory<TreatmentSessionDbContext>
{
    /// <inheritdoc />
    public TreatmentSessionDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TreatmentSessionDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=treatment_session_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new TreatmentSessionDbContext(optionsBuilder.Options);
    }
}
