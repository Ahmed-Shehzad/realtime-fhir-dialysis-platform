using BuildingBlocks.Abstractions;

namespace RealtimeSurveillance.Domain.Abstractions;

public interface ISessionRiskSnapshotRepository : IRepository<RealtimeSurveillance.Domain.SessionRiskSnapshot>
{
    Task<RealtimeSurveillance.Domain.SessionRiskSnapshot?> GetBySessionIdForUpdateAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);

    Task<RealtimeSurveillance.Domain.SessionRiskSnapshot?> GetBySessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);
}
