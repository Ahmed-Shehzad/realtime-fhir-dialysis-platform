namespace DeviceRegistry.Application.Commands.RegisterDevice;

/// <summary>
/// Result of a successful device registration.
/// </summary>
public sealed record RegisterDeviceResult(Ulid AggregateId, string DeviceId, string TrustState);
