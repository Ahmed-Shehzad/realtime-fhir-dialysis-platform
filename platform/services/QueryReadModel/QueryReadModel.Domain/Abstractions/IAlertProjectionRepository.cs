using BuildingBlocks.Abstractions;

namespace QueryReadModel.Domain.Abstractions;

public interface IAlertProjectionRepository : IRepository<QueryReadModel.Domain.AlertProjection>
{
    Task<QueryReadModel.Domain.AlertProjection?> GetByAlertRowKeyForUpdateAsync(
        string alertRowKey,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<QueryReadModel.Domain.AlertProjection>> ListAsync(
        string? severityFilter,
        CancellationToken cancellationToken = default);

    Task<int> CountOpenAsync(CancellationToken cancellationToken = default);
}
