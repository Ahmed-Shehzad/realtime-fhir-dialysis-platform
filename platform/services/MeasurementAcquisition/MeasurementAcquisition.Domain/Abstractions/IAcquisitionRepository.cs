using BuildingBlocks.Abstractions;

namespace MeasurementAcquisition.Domain.Abstractions;

/// <summary>
/// Persistence for <see cref="RawMeasurementEnvelope"/>.
/// </summary>
public interface IAcquisitionRepository : IRepository<MeasurementAcquisition.Domain.RawMeasurementEnvelope>
{
}
