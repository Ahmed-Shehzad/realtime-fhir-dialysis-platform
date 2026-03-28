using BuildingBlocks.Interceptors;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.Persistence;

public static class EfCoreBoundedContextServiceCollectionExtensions
{
    public static IServiceCollection AddNpgsqlDbContextWithOutboxInterceptors<TDbContext>(
        this IServiceCollection services,
        string connectionString)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        return services.AddDbContext<TDbContext>((serviceProvider, options) =>
        {
            _ = options.UseNpgsql(connectionString);
            _ = options.AddInterceptors(
                serviceProvider.GetRequiredService<DomainEventDispatcherInterceptor>(),
                serviceProvider.GetRequiredService<IntegrationEventDispatcherInterceptor>());
        });
    }

    public static IServiceCollection AddNpgsqlBoundedContext<TDbContext>(
        this IServiceCollection services,
        string connectionString)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.AddNpgsqlDbContextWithOutboxInterceptors<TDbContext>(connectionString);
        return services;
    }
}
