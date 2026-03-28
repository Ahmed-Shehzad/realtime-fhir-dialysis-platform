using SignalConditioning.Domain;
using SignalConditioning.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace SignalConditioning.Infrastructure.Persistence;

public sealed class ConditioningResultRepository : Repository<ConditioningResult>, IConditioningResultRepository
{
    private readonly SignalConditioningDbContext _db;

    public ConditioningResultRepository(SignalConditioningDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public async Task<ConditioningResult?> GetLatestByMeasurementIdAsync(
        string measurementId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(measurementId))
            return null;
        string id = measurementId.Trim();
        return await _db.ConditioningResults
            .AsNoTracking()
            .Where(e => e.MeasurementId == id)
            .OrderByDescending(e => e.EvaluatedAtUtc)
            .ThenByDescending(e => e.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
