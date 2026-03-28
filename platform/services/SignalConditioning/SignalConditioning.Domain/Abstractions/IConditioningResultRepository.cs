using BuildingBlocks.Abstractions;

namespace SignalConditioning.Domain.Abstractions;

public interface IConditioningResultRepository : IRepository<ConditioningResult>
{
    Task<ConditioningResult?> GetLatestByMeasurementIdAsync(
        string measurementId,
        CancellationToken cancellationToken = default);
}
