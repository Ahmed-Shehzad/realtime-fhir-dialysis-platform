using BuildingBlocks.Abstractions;

namespace ClinicalInteroperability.Domain.Abstractions;

public interface ICanonicalObservationPublicationRepository : IRepository<ClinicalInteroperability.Domain.CanonicalObservationPublication>
{
    Task<ClinicalInteroperability.Domain.CanonicalObservationPublication?> GetLatestByMeasurementIdAsync(
        string measurementId,
        CancellationToken cancellationToken = default);

    Task<ClinicalInteroperability.Domain.CanonicalObservationPublication?> GetByIdForUpdateAsync(
        Ulid publicationId,
        CancellationToken cancellationToken = default);
}
