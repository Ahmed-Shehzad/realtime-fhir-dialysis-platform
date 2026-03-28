using MassTransit;

using Microsoft.Extensions.Options;

namespace RealtimePlatform.MassTransit.Consumers;

/// <summary>Maps <see cref="IntercessorCommandConsumer"/> to the configured Azure Service Bus queue (or in-memory equivalent).</summary>
public sealed class IntercessorCommandConsumerDefinition : ConsumerDefinition<IntercessorCommandConsumer>
{
    public IntercessorCommandConsumerDefinition(IOptions<MassTransitAzureServiceBusOptions> options)
    {
        EndpointName = options.Value.IntercessorCommandsQueueName;
    }
}
