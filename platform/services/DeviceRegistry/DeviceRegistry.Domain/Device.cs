using BuildingBlocks;

using BuildingBlocks.ValueObjects;

using DeviceRegistry.Domain.Events;
using DeviceRegistry.Domain.IntegrationEvents;

namespace DeviceRegistry.Domain;

/// <summary>
/// Device aggregate root (onboarding and trust).
/// </summary>
public sealed class Device : AggregateRoot
{
    /// <summary>
    /// Maximum length for <see cref="Manufacturer"/> (single slice constraint).
    /// </summary>
    public const int MaxManufacturerLength = 256;

    /// <summary>
    /// EF Core materialization constructor.
    /// </summary>
    private Device()
    {
        DeviceId = new DeviceId("__uninitialized__");
        TrustState = TrustState.Active;
    }

    public DeviceId DeviceId { get; private set; }

    public TrustState TrustState { get; private set; }

    public string Manufacturer { get; private set; } = string.Empty;

    /// <summary>
    /// Registers a new device and queues domain + integration events.
    /// </summary>
    /// <param name="correlationId">Correlation for tracing and integration events.</param>
    /// <param name="deviceId">Business device identifier.</param>
    /// <param name="trustState">Initial trust state.</param>
    /// <param name="manufacturer">Optional manufacturer label.</param>
    /// <param name="tenantId">Resolved tenant for integration envelope metadata; may be null for non-HTTP callers.</param>
    public static Device Register(
        Ulid correlationId,
        DeviceId deviceId,
        TrustState trustState,
        string? manufacturer,
        string? tenantId)
    {
        var device = new Device
        {
            DeviceId = deviceId,
            TrustState = trustState,
            Manufacturer = manufacturer ?? string.Empty
        };
        device.ApplyEvent(new DeviceRegisteredDomainEvent(deviceId));
        device.ApplyEvent(
            new DeviceRegisteredIntegrationEvent(correlationId, deviceId.Value, trustState.Value)
            {
                RoutingDeviceId = deviceId.Value,
                TenantId = tenantId
            });
        return device;
    }

    /// <summary>
    /// Suspends a previously active device.
    /// </summary>
    public void Suspend()
    {
        TrustState = TrustState.Suspended;
        ApplyUpdateDateTime();
    }
}
