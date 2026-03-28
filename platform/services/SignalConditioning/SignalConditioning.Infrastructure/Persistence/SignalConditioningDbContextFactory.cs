using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SignalConditioning.Infrastructure.Persistence;

public sealed class SignalConditioningDbContextFactory : IDesignTimeDbContextFactory<SignalConditioningDbContext>
{
    public SignalConditioningDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<SignalConditioningDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=signal_conditioning_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new SignalConditioningDbContext(optionsBuilder.Options);
    }
}
