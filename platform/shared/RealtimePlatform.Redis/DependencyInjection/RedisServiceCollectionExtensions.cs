using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using StackExchange.Redis;

namespace RealtimePlatform.Redis.DependencyInjection;

/// <summary>Registers Redis per ADR-0001 for cache-aside and optional health reporting.</summary>
public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Binds <see cref="RedisPlatformOptions"/> from <c>RealtimePlatform:Redis</c> (override via <paramref name="configurationSectionPath"/>).
    /// When <see cref="RedisPlatformOptions.Enabled"/> is true, registers <see cref="IConnectionMultiplexer"/> as singleton and stack-exchange distributed cache.
    /// </summary>
    public static IServiceCollection AddRealtimePlatformRedis(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "RealtimePlatform:Redis")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationSectionPath);

        IConfigurationSection section = configuration.GetSection(configurationSectionPath);
        _ = services.Configure<RedisPlatformOptions>(section);

        RedisPlatformOptions snapshot = section.Get<RedisPlatformOptions>() ?? new RedisPlatformOptions();
        if (!snapshot.Enabled)
            return services;

        string? conn = !string.IsNullOrWhiteSpace(snapshot.ConnectionString)
            ? snapshot.ConnectionString
            : configuration.GetConnectionString("Redis");
        ArgumentException.ThrowIfNullOrWhiteSpace(conn);

        string instanceName = string.IsNullOrWhiteSpace(snapshot.InstanceName)
            ? new RedisPlatformOptions().InstanceName
            : snapshot.InstanceName;

        _ = services.AddSingleton<IConnectionMultiplexer>(_ => ConnectionMultiplexer.Connect(conn));
        _ = services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = conn;
            options.InstanceName = instanceName;
        });

        return services;
    }

    /// <summary>Adds a Redis health check when Redis is enabled (same section binding as <see cref="AddRealtimePlatformRedis"/>).</summary>
    public static IHealthChecksBuilder AddRealtimePlatformRedisHealthCheck(
        this IHealthChecksBuilder healthChecks,
        IConfiguration configuration,
        string configurationSectionPath = "RealtimePlatform:Redis",
        string name = "redis")
    {
        ArgumentNullException.ThrowIfNull(healthChecks);
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection(configurationSectionPath);
        RedisPlatformOptions snapshot = section.Get<RedisPlatformOptions>() ?? new RedisPlatformOptions();
        if (!snapshot.Enabled)
            return healthChecks;

        string? conn = !string.IsNullOrWhiteSpace(snapshot.ConnectionString)
            ? snapshot.ConnectionString
            : configuration.GetConnectionString("Redis");
        if (string.IsNullOrWhiteSpace(conn))
            return healthChecks;

        return healthChecks.AddRedis(conn, name: name);
    }
}
