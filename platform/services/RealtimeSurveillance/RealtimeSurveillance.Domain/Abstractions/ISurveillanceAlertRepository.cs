using BuildingBlocks.Abstractions;

namespace RealtimeSurveillance.Domain.Abstractions;

public interface ISurveillanceAlertRepository : IRepository<RealtimeSurveillance.Domain.SurveillanceAlert>
{
    Task<RealtimeSurveillance.Domain.SurveillanceAlert?> GetByIdForUpdateAsync(Ulid alertId, CancellationToken cancellationToken = default);

    Task<RealtimeSurveillance.Domain.SurveillanceAlert?> GetByIdAsync(Ulid alertId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RealtimeSurveillance.Domain.SurveillanceAlert>> ListBySessionAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);
}
