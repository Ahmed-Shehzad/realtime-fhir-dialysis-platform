using System.Text.Json;

using Intercessor.Abstractions;

using MassTransit;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RealtimePlatform.MassTransit.Intercessor;

namespace RealtimePlatform.MassTransit.Consumers;

/// <summary>Receives <see cref="IntercessorCommandEnvelope"/> from Azure Service Bus (or in-memory) and dispatches to <see cref="ISender"/>.</summary>
public sealed class IntercessorCommandConsumer : IConsumer<IntercessorCommandEnvelope>
{
    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
    };

    private readonly ISender _sender;
    private readonly IOptions<MassTransitAzureServiceBusOptions> _options;
    private readonly ILogger<IntercessorCommandConsumer> _logger;

    public IntercessorCommandConsumer(
        ISender sender,
        IOptions<MassTransitAzureServiceBusOptions> options,
        ILogger<IntercessorCommandConsumer> logger)
    {
        _sender = sender;
        _options = options;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<IntercessorCommandEnvelope> context)
    {
        IntercessorCommandEnvelope envelope = context.Message;
        if (string.IsNullOrWhiteSpace(envelope.RequestTypeAssemblyQualifiedName)
            || string.IsNullOrWhiteSpace(envelope.PayloadJson))
        {
            _logger.LogWarning(
                "Intercessor command envelope rejected: missing type or payload. CorrelationId={CorrelationId}",
                envelope.CorrelationId);
            throw new InvalidOperationException("Intercessor command envelope is missing type or payload.");
        }

        Type? requestType = Type.GetType(
            envelope.RequestTypeAssemblyQualifiedName,
            throwOnError: false,
            ignoreCase: false);
        if (requestType is null)
        {
            _logger.LogWarning(
                "Intercessor command type not resolved: {Type}. CorrelationId={CorrelationId}",
                envelope.RequestTypeAssemblyQualifiedName,
                envelope.CorrelationId);
            throw new InvalidOperationException("Command type could not be loaded.");
        }

        if (!DurableIntercessorCommandRules.IsCommandTypeAllowed(requestType, _options.Value))
        {
            _logger.LogWarning(
                "Intercessor command type rejected by policy: {Type}. CorrelationId={CorrelationId}",
                requestType.FullName,
                envelope.CorrelationId);
            throw new InvalidOperationException("Command type is not allowed by configuration.");
        }

        object? request;
        try
        {
            request = JsonSerializer.Deserialize(envelope.PayloadJson, requestType, JsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(
                ex,
                "Intercessor command JSON deserialize failed for {Type}. CorrelationId={CorrelationId}",
                requestType.FullName,
                envelope.CorrelationId);
            throw new InvalidOperationException(
                $"Intercessor command JSON deserialize failed for {requestType.FullName}. CorrelationId={envelope.CorrelationId}",
                ex);
        }

        if (request is null)
        {
            _logger.LogWarning(
                "Intercessor command deserialized to null for {Type}. CorrelationId={CorrelationId}",
                requestType.FullName,
                envelope.CorrelationId);
            throw new InvalidOperationException("Deserialized command was null.");
        }

        await IntercessorSenderInvoke.DispatchAsync(_sender, request, context.CancellationToken).ConfigureAwait(false);
    }
}
