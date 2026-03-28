using System.Reflection;

using MassTransit;

using Microsoft.Extensions.Options;

using RealtimePlatform.MassTransit;

namespace BuildingBlocks.MassTransit;

/// <summary>Names the receive endpoint (Azure subscription name) for <see cref="CatalogIntegrationEventDispatchConsumer"/>.</summary>
public sealed class CatalogIntegrationEventDispatchConsumerDefinition
    : ConsumerDefinition<CatalogIntegrationEventDispatchConsumer>
{
    public CatalogIntegrationEventDispatchConsumerDefinition(IOptions<MassTransitAzureServiceBusOptions> options)
    {
        string? configured = options.Value.IntegrationEventsSubscriptionName;
        EndpointName = string.IsNullOrWhiteSpace(configured)
            ? $"{SanitizeEntityPathSegment(Assembly.GetEntryAssembly()?.GetName().Name ?? "service")}-integration-events"
            : configured;
    }

    private static string SanitizeEntityPathSegment(string name)
    {
        ReadOnlySpan<char> span = name;
        Span<char> buffer = stackalloc char[span.Length];
        int length = 0;
        foreach (char c in span)
            if (char.IsAsciiLetterOrDigit(c) || c is '-' or '_' or '.')
                buffer[length++] = char.ToLowerInvariant(c);
            else
                buffer[length++] = '-';

        return new string(buffer[..length]);
    }
}
