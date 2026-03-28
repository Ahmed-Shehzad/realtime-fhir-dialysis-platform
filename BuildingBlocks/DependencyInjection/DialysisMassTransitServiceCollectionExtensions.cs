using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using BuildingBlocks.MassTransit;

using RealtimePlatform.MassTransit;
using RealtimePlatform.MassTransit.DependencyInjection;

namespace BuildingBlocks.DependencyInjection;

/// <summary>Registers MassTransit with platform outbox/inbox plus catalog integration event subscription consumer.</summary>
public static class DialysisMassTransitServiceCollectionExtensions
{
    /// <inheritdoc cref="MassTransitServiceCollectionExtensions.AddRealtimePlatformMassTransit{TDbContext}" />
    public static IServiceCollection AddDialysisPlatformMassTransit<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureAdditionalBusRegistrations = null,
        string configurationSectionPath = MassTransitAzureServiceBusOptions.SectionPath)
        where TDbContext : DbContext
    {
        return services.AddRealtimePlatformMassTransit<TDbContext>(
            configuration,
            bus =>
            {
                _ = bus.AddConsumer<
                    CatalogIntegrationEventDispatchConsumer,
                    CatalogIntegrationEventDispatchConsumerDefinition>();
                configureAdditionalBusRegistrations?.Invoke(bus);
            },
            configurationSectionPath);
    }
}
