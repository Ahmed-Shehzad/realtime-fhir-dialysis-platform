using BuildingBlocks.ValueObjects;

using DeviceRegistry.Domain;
using DeviceRegistry.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using DeviceRegistry.Infrastructure.Persistence;

namespace DeviceRegistry.Infrastructure.Trust;

/// <summary>
/// Trust evaluation backed by the registry database.
/// </summary>
public sealed class TrustLookupService : ITrustLookup
{
    private readonly DeviceRegistryDbContext _db;

    /// <summary>
    /// Creates a lookup service.
    /// </summary>
    public TrustLookupService(DeviceRegistryDbContext db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public async Task<bool> IsDeviceTrustedAsync(DeviceId deviceId, CancellationToken cancellationToken = default)
    {
        Device? device = await _db.Devices
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.DeviceId == deviceId, cancellationToken)
            .ConfigureAwait(false);
        return device is not null && device.TrustState.Value == TrustState.Active.Value;
    }
}
