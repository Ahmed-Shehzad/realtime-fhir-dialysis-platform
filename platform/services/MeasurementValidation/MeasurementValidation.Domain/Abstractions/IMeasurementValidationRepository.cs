using BuildingBlocks.Abstractions;

namespace MeasurementValidation.Domain.Abstractions;

public interface IMeasurementValidationRepository : IRepository<MeasurementValidation.Domain.ValidatedMeasurement>
{
    Task<MeasurementValidation.Domain.ValidatedMeasurement?> GetLatestByMeasurementIdAsync(string measurementId, CancellationToken cancellationToken = default);
}
