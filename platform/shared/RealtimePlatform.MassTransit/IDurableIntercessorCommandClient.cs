using Intercessor.Abstractions;

namespace RealtimePlatform.MassTransit;

/// <summary>Enqueues an Intercessor <see cref="IRequest"/> on the durable MassTransit / Service Bus queue for asynchronous execution by the Intercessor command consumer.</summary>
public interface IDurableIntercessorCommandClient
{
    /// <summary>
    /// Sends the command to the configured queue. The handler runs asynchronously in a consumer; there is no response payload to the caller.
    /// </summary>
    Task EnqueueAsync(IRequest command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the command with optional tenant and correlation metadata (C5 / observability).
    /// </summary>
    Task EnqueueAsync(
        IRequest command,
        string? tenantId,
        string? correlationId,
        CancellationToken cancellationToken = default);
}
