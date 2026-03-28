using BuildingBlocks.Abstractions;
using BuildingBlocks.ValueObjects;

namespace DeviceRegistry.Domain.Abstractions;

/// <summary>
/// Persistence port for <see cref="Device"/>.
/// </summary>
public interface IDeviceRepository : IRepository<DeviceRegistry.Domain.Device>
{
    Task<DeviceRegistry.Domain.Device?> GetByDeviceIdAsync(DeviceId deviceId, CancellationToken cancellationToken = default);
}
