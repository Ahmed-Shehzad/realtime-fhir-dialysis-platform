using BuildingBlocks;

namespace DeviceRegistry.Domain.IntegrationEvents;

/// <summary>
/// Integration event published after a device is persisted (outbox).
/// </summary>
/// <param name="CorrelationId">Correlation for tracing.</param>
/// <param name="DeviceId">Business device identifier.</param>
/// <param name="TrustState">Trust state value at registration.</param>
public sealed record DeviceRegisteredIntegrationEvent(
    Ulid CorrelationId,
    string DeviceId,
    string TrustState) : IntegrationEvent(CorrelationId);
