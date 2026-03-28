using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FinancialInteroperability.Infrastructure.Persistence;

public sealed class FinancialInteroperabilityDbContextFactory
    : IDesignTimeDbContextFactory<FinancialInteroperabilityDbContext>
{
    public FinancialInteroperabilityDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<FinancialInteroperabilityDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=financial_interoperability_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new FinancialInteroperabilityDbContext(optionsBuilder.Options);
    }
}
