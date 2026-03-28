using ClinicalInteroperability.Domain;
using ClinicalInteroperability.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace ClinicalInteroperability.Infrastructure.Persistence;

public sealed class CanonicalObservationPublicationRepository : Repository<CanonicalObservationPublication>, ICanonicalObservationPublicationRepository
{
    private readonly ClinicalInteroperabilityDbContext _db;

    public CanonicalObservationPublicationRepository(ClinicalInteroperabilityDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<CanonicalObservationPublication?> GetLatestByMeasurementIdAsync(
        string measurementId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(measurementId))
            return null;
        string id = measurementId.Trim();
        return await _db.CanonicalObservationPublications
            .AsNoTracking()
            .Where(p => p.MeasurementId == id)
            .OrderByDescending(p => p.LastAttemptAtUtc)
            .ThenByDescending(p => p.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public Task<CanonicalObservationPublication?> GetByIdForUpdateAsync(
        Ulid publicationId,
        CancellationToken cancellationToken = default) =>
        _db.CanonicalObservationPublications
            .FirstOrDefaultAsync(p => p.Id == publicationId, cancellationToken);
}
