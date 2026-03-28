using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Persistence;

/// <summary>
/// Registers PostgreSQL health checks against <see cref="IConfiguration"/> so Aspire-injected
/// <c>ConnectionStrings__*</c> (and other late configuration) is visible when the check runs, not only at startup registration.
/// </summary>
public static class NpgsqlDefaultConnectionHealthCheckExtensions
{
    public static IHealthChecksBuilder AddNpgsqlDefaultConnectionHealthCheck(
        this IHealthChecksBuilder healthChecks,
        string connectionName = "Default",
        string checkName = "postgres")
    {
        ArgumentNullException.ThrowIfNull(healthChecks);
        return healthChecks.AddNpgSql(
            serviceProvider =>
            {
                IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
                return configuration.GetConnectionString(connectionName)
                    ?? throw new InvalidOperationException(
                        $"ConnectionStrings:{connectionName} is required for health checks.");
            },
            name: checkName);
    }
}
