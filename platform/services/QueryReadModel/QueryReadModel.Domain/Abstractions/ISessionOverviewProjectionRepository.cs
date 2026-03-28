using BuildingBlocks.Abstractions;

namespace QueryReadModel.Domain.Abstractions;

public interface ISessionOverviewProjectionRepository : IRepository<QueryReadModel.Domain.SessionOverviewProjection>
{
    Task<QueryReadModel.Domain.SessionOverviewProjection?> GetByTreatmentSessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);

    Task<QueryReadModel.Domain.SessionOverviewProjection?> GetByTreatmentSessionIdForUpdateAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);
}
