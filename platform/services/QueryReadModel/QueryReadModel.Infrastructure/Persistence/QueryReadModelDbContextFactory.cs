using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class QueryReadModelDbContextFactory : IDesignTimeDbContextFactory<QueryReadModelDbContext>
{
    public QueryReadModelDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<QueryReadModelDbContext>();
        string connectionString = Environment.GetEnvironmentVariable("REALTIME_PLATFORM_EF_CONNECTION")
            ?? "Host=127.0.0.1;Port=5432;Database=query_read_model_dev;Username=postgres;Password=postgres";
        _ = optionsBuilder.UseNpgsql(connectionString);
        return new QueryReadModelDbContext(optionsBuilder.Options);
    }
}
