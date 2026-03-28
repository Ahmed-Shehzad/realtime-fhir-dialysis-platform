namespace RealtimePlatform.MassTransit;

/// <summary>Configuration for Azure Service Bus when used with MassTransit v8 (OSS).</summary>
public sealed class MassTransitAzureServiceBusOptions
{
    /// <summary>Configuration section path (colon-separated).</summary>
    public const string SectionPath = "RealtimePlatform:MassTransit:AzureServiceBus";

    /// <summary>Service Bus connection string. When null or whitespace, MassTransit registration is skipped.</summary>
    public string? ConnectionString { get; set; }

    /// <summary>Topic name for <see cref="CatalogIntegrationEventTransport"/> publishes.</summary>
    public string IntegrationEventsTopicName { get; set; } = "integration-events";

    /// <summary>
    /// Azure Service Bus subscription name for this host under <see cref="IntegrationEventsTopicName"/>.
    /// Must be unique per microservice (and stable per deployment) so each subscriber receives its own copy of integration events.
    /// When null or whitespace, MassTransit uses <c>{entry-assembly-name}-integration-events</c>.
    /// </summary>
    /// <remarks>
    /// C5: restrict topic/subscription management to MI/RBAC; do not use anonymous listen rules.
    /// Namespace SAS keys are suitable for dev only — prefer Azure Entra workload identity in shared environments.
    /// </remarks>
    public string? IntegrationEventsSubscriptionName { get; set; }

    /// <summary>Queue that receives <see cref="IntercessorCommandEnvelope"/> for durable Intercessor dispatch.</summary>
    public string IntercessorCommandsQueueName { get; set; } = "intercessor-commands";

    /// <summary>
    /// When non-empty, only command types whose assembly short name starts with one of these prefixes (ordinal-ignore-case)
    /// may be enqueued or consumed. When empty, types in System.* / Microsoft.* assemblies are rejected; all other loaded assemblies are allowed.
    /// </summary>
    public List<string> AllowedCommandAssemblyNamePrefixes { get; set; } = [];
}
