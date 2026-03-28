using BuildingBlocks;

using BuildingBlocks.ValueObjects;

namespace DeviceRegistry.Domain.Events;

/// <summary>
/// Domain event raised when a device is registered in the registry.
/// </summary>
/// <param name="DeviceId">Business device identifier.</param>
public sealed record DeviceRegisteredDomainEvent(DeviceId DeviceId) : DomainEvent;
