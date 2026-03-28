using MeasurementAcquisition.Domain;
using MeasurementAcquisition.Domain.Abstractions;

using BuildingBlocks;

namespace MeasurementAcquisition.Infrastructure.Persistence;

/// <summary>
/// EF Core repository for raw measurement envelopes.
/// </summary>
public sealed class AcquisitionRepository : Repository<RawMeasurementEnvelope>, IAcquisitionRepository
{
    /// <summary>
    /// Creates a repository.
    /// </summary>
    public AcquisitionRepository(MeasurementAcquisitionDbContext db) : base(db)
    {
        ArgumentNullException.ThrowIfNull(db);
    }
}
