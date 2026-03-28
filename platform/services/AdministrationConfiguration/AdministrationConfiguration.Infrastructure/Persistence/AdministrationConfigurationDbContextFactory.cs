using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class AdministrationConfigurationDbContextFactory : IDesignTimeDbContextFactory<AdministrationConfigurationDbContext>
{
    public AdministrationConfigurationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AdministrationConfigurationDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
            ?? Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=administration_configuration_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new AdministrationConfigurationDbContext(optionsBuilder.Options);
    }
}
