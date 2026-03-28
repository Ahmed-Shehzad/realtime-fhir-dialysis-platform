using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using Intercessor.Abstractions;

namespace QueryReadModel.Application.Queries.ListAlertProjections;

public sealed class ListAlertProjectionsQueryHandler
    : IQueryHandler<ListAlertProjectionsQuery, IReadOnlyList<AlertProjectionReadDto>>
{
    private readonly IAlertProjectionRepository _repository;

    public ListAlertProjectionsQueryHandler(IAlertProjectionRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IReadOnlyList<AlertProjectionReadDto>> HandleAsync(
        ListAlertProjectionsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        IReadOnlyList<AlertProjection> rows = await _repository
            .ListAsync(query.SeverityFilter, cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(r => new AlertProjectionReadDto(
                r.Id.ToString(),
                r.AlertRowKey,
                r.AlertType,
                r.Severity,
                r.AlertState,
                r.TreatmentSessionId,
                r.RaisedAtUtc,
                r.ProjectionUpdatedAtUtc))
            .ToList();
    }
}
