using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ReplayRecovery.Infrastructure.Persistence;

public sealed class ReplayRecoveryDbContextFactory : IDesignTimeDbContextFactory<ReplayRecoveryDbContext>
{
    public ReplayRecoveryDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ReplayRecoveryDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=replay_recovery_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new ReplayRecoveryDbContext(optionsBuilder.Options);
    }
}
