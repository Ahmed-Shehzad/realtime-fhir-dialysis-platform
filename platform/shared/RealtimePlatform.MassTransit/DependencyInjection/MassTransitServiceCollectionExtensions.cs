using MassTransit;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using RealtimePlatform.MassTransit.Consumers;

namespace RealtimePlatform.MassTransit.DependencyInjection;

/// <summary>Registers MassTransit v8 with EF Core transactional outbox + inbox and Azure Service Bus (or in-memory transport when the connection string is unset).</summary>
public static class MassTransitServiceCollectionExtensions
{
    /// <summary>Adds MassTransit with EF Core transactional outbox, bus outbox delivery, and consumer inbox/outbox for <typeparamref name="TDbContext"/>.</summary>
    /// <param name="services">Application services.</param>
    /// <param name="configuration">Configuration root.</param>
    /// <param name="configureBusRegistration">Optional extra consumers, sagas, or bus wiring (runs before transport selection).</param>
    /// <param name="configurationSectionPath">Section binding path for <see cref="MassTransitAzureServiceBusOptions"/>.</param>
    public static IServiceCollection AddRealtimePlatformMassTransit<TDbContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureBusRegistration = null,
        string configurationSectionPath = MassTransitAzureServiceBusOptions.SectionPath)
        where TDbContext : DbContext
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        IConfigurationSection section = configuration.GetSection(configurationSectionPath);
        _ = services.Configure<MassTransitAzureServiceBusOptions>(section);
        MassTransitAzureServiceBusOptions snapshot =
            section.Get<MassTransitAzureServiceBusOptions>() ?? new MassTransitAzureServiceBusOptions();

        _ = services.AddSingleton<IDurableIntercessorCommandClient, DurableIntercessorCommandClient>();

        return services.AddMassTransit(busConfigurator =>
        {
            busConfigurator.SetKebabCaseEndpointNameFormatter();
            busConfigurator.AddEntityFrameworkOutbox<TDbContext>(outboxConfigurator =>
                outboxConfigurator.UsePostgres().UseBusOutbox());

            busConfigurator.AddConfigureEndpointsCallback((context, name, cfg) =>
                cfg.UseEntityFrameworkOutbox<TDbContext>(context));

            _ = busConfigurator.AddConsumer<IntercessorCommandConsumer, IntercessorCommandConsumerDefinition>();

            configureBusRegistration?.Invoke(busConfigurator);

            if (string.IsNullOrWhiteSpace(snapshot.ConnectionString))
                busConfigurator.UsingInMemory(
                    (context, cfg) =>
                    {
                        MassTransitAzureServiceBusOptions options = context
                            .GetRequiredService<IOptions<MassTransitAzureServiceBusOptions>>()
                            .Value;
                        cfg.Message<CatalogIntegrationEventTransport>(topology =>
                            topology.SetEntityName(options.IntegrationEventsTopicName));
                        cfg.ConfigureEndpoints(context);
                    });
            else
                busConfigurator.UsingAzureServiceBus(
                    (context, cfg) =>
                    {
                        MassTransitAzureServiceBusOptions options = context
                            .GetRequiredService<IOptions<MassTransitAzureServiceBusOptions>>()
                            .Value;
                        cfg.Host(options.ConnectionString);
                        cfg.Message<CatalogIntegrationEventTransport>(topology =>
                            topology.SetEntityName(options.IntegrationEventsTopicName));
                        cfg.ConfigureEndpoints(context);
                    });
        });
    }
}
