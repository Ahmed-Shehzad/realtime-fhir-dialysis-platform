using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using Intercessor.Abstractions;

namespace QueryReadModel.Application.Queries.GetSessionOverview;

public sealed class GetSessionOverviewQueryHandler : IQueryHandler<GetSessionOverviewQuery, SessionOverviewReadDto?>
{
    private readonly ISessionOverviewProjectionRepository _repository;

    public GetSessionOverviewQueryHandler(ISessionOverviewProjectionRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<SessionOverviewReadDto?> HandleAsync(
        GetSessionOverviewQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        SessionOverviewProjection? row = await _repository
            .GetByTreatmentSessionIdAsync(query.TreatmentSessionId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;
        return new SessionOverviewReadDto(
            row.Id.ToString(),
            row.TreatmentSessionId,
            row.SessionState,
            row.PatientDisplayLabel,
            row.LinkedDeviceId,
            row.SessionStartedAtUtc,
            row.ProjectionUpdatedAtUtc);
    }
}
