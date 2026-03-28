using BuildingBlocks.ValueObjects;

namespace DeviceRegistry.Domain.Abstractions;

/// <summary>
/// Read-only trust contract for downstream services (least privilege lookup).
/// </summary>
public interface ITrustLookup
{
    /// <summary>
    /// Returns true when the device exists and is in <see cref="TrustState.Active"/>.
    /// </summary>
    Task<bool> IsDeviceTrustedAsync(DeviceId deviceId, CancellationToken cancellationToken = default);
}
