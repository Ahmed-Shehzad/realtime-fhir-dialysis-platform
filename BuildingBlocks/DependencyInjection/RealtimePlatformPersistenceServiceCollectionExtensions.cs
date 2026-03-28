using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using RealtimePlatform.Messaging.DependencyInjection;

namespace BuildingBlocks.DependencyInjection;

/// <summary>
/// Registers JSON messaging and optional integration event publisher options (for example headers).
/// </summary>
public static class RealtimePlatformPersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Adds JSON <see cref="RealtimePlatform.Messaging.IMessageSerializer"/> and binds <see cref="OutboxPublisherOptions"/>.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">Application configuration.</param>
    /// <param name="configurationSectionPath">Root section (default <c>RealtimePlatform</c>) for child key <c>OutboxPublisher</c>.</param>
    /// <remarks>
    /// Integration events are staged via MassTransit EF transactional outbox in <c>IntegrationEventDispatcherInterceptor</c>.
    /// Register domain and integration interceptors on <see cref="Microsoft.EntityFrameworkCore.DbContextOptionsBuilder"/> in dependency order.
    /// </remarks>
    public static IServiceCollection AddRealtimePlatformMessagingPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionPath = "RealtimePlatform")
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationSectionPath);

        _ = services.AddRealtimePlatformJsonSerialization();
        _ = services.Configure<OutboxPublisherOptions>(
            configuration.GetSection($"{configurationSectionPath}:OutboxPublisher"));
        return services;
    }
}
