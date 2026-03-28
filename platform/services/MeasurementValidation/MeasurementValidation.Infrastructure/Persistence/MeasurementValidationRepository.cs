using MeasurementValidation.Domain;
using MeasurementValidation.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace MeasurementValidation.Infrastructure.Persistence;

public sealed class MeasurementValidationRepository : Repository<ValidatedMeasurement>, IMeasurementValidationRepository
{
    private readonly MeasurementValidationDbContext _db;

    public MeasurementValidationRepository(MeasurementValidationDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<ValidatedMeasurement?> GetLatestByMeasurementIdAsync(
        string measurementId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(measurementId))
            return null;
        string id = measurementId.Trim();
        return await _db.ValidatedMeasurements
            .AsNoTracking()
            .Where(v => v.MeasurementId == id)
            .OrderByDescending(v => v.EvaluatedAtUtc)
            .ThenByDescending(v => v.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
