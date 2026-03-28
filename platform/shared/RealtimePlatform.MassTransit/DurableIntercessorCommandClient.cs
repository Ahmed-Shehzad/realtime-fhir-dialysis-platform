using System.Diagnostics;
using System.Text.Json;

using Intercessor.Abstractions;

using MassTransit;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace RealtimePlatform.MassTransit;

internal sealed class DurableIntercessorCommandClient : IDurableIntercessorCommandClient
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IOptions<MassTransitAzureServiceBusOptions> _options;

    public DurableIntercessorCommandClient(
        IServiceScopeFactory serviceScopeFactory,
        IOptions<MassTransitAzureServiceBusOptions> options)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _options = options;
    }

    public Task EnqueueAsync(IRequest command, CancellationToken cancellationToken = default) =>
        EnqueueAsync(command, tenantId: null, correlationId: null, cancellationToken);

    public async Task EnqueueAsync(
        IRequest command,
        string? tenantId,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        Type runtimeType = command.GetType();
        if (!DurableIntercessorCommandRules.IsCommandTypeAllowed(runtimeType, _options.Value))
            throw new InvalidOperationException($"Command type is not allowed for durable enqueue: {runtimeType.FullName}");

        string json = JsonSerializer.Serialize(command, runtimeType, JsonSerializerOptions);
        string? effectiveCorrelation = correlationId ?? Activity.Current?.Id;
        var envelope = new IntercessorCommandEnvelope
        {
            RequestTypeAssemblyQualifiedName = runtimeType.AssemblyQualifiedName
                ?? throw new InvalidOperationException("Command type has no assembly-qualified name."),
            PayloadJson = json,
            CorrelationId = effectiveCorrelation,
            TenantId = tenantId,
        };

        MassTransitAzureServiceBusOptions options = _options.Value;
        string queueName = options.IntercessorCommandsQueueName;
        var queueUri = new Uri($"queue:{queueName}");
        await using AsyncServiceScope scope = _serviceScopeFactory.CreateAsyncScope();
        ISendEndpointProvider sendEndpointProvider =
            scope.ServiceProvider.GetRequiredService<ISendEndpointProvider>();
        ISendEndpoint endpoint = await sendEndpointProvider
            .GetSendEndpoint(queueUri)
            .ConfigureAwait(false);
        await endpoint.Send(envelope, cancellationToken).ConfigureAwait(false);
    }
}
