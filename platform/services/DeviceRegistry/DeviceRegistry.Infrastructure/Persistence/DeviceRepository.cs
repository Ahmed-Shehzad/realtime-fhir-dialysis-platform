using BuildingBlocks;
using BuildingBlocks.ValueObjects;

using DeviceRegistry.Domain;
using DeviceRegistry.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace DeviceRegistry.Infrastructure.Persistence;

/// <summary>
/// EF Core repository for devices.
/// </summary>
public sealed class DeviceRepository : Repository<Device>, IDeviceRepository
{
    private readonly DeviceRegistryDbContext _db;

    /// <summary>
    /// Creates a repository.
    /// </summary>
    public DeviceRepository(DeviceRegistryDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public Task<Device?> GetByDeviceIdAsync(DeviceId deviceId, CancellationToken cancellationToken = default) =>
        _db.Devices.FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken);
}
