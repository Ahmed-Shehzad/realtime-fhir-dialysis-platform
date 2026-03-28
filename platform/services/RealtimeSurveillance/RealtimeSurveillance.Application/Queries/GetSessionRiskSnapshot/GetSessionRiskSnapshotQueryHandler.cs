using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Queries.GetSessionRiskSnapshot;

public sealed class GetSessionRiskSnapshotQueryHandler : IQueryHandler<GetSessionRiskSnapshotQuery, SessionRiskReadDto?>
{
    private readonly ISessionRiskSnapshotRepository _snapshots;

    public GetSessionRiskSnapshotQueryHandler(ISessionRiskSnapshotRepository snapshots) =>
        _snapshots = snapshots ?? throw new ArgumentNullException(nameof(snapshots));

    public async Task<SessionRiskReadDto?> HandleAsync(
        GetSessionRiskSnapshotQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.TreatmentSessionId))
            return null;

        SessionRiskSnapshot? row = await _snapshots
            .GetBySessionIdAsync(query.TreatmentSessionId.Trim(), cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;
        return new SessionRiskReadDto(
            row.Id.ToString(),
            row.TreatmentSessionId.Value,
            row.RiskLevel.Value,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
