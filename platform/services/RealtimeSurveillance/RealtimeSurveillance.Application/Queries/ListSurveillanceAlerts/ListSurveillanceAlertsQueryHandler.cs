using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Queries.ListSurveillanceAlerts;

public sealed class ListSurveillanceAlertsQueryHandler
    : IQueryHandler<ListSurveillanceAlertsQuery, IReadOnlyList<SurveillanceAlertListItemDto>>
{
    private readonly ISurveillanceAlertRepository _alerts;

    public ListSurveillanceAlertsQueryHandler(ISurveillanceAlertRepository alerts) =>
        _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));

    public async Task<IReadOnlyList<SurveillanceAlertListItemDto>> HandleAsync(
        ListSurveillanceAlertsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (string.IsNullOrWhiteSpace(query.TreatmentSessionId)) return Array.Empty<SurveillanceAlertListItemDto>();

        IReadOnlyList<SurveillanceAlert> rows = await _alerts
            .ListBySessionAsync(query.TreatmentSessionId.Trim(), cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(r => new SurveillanceAlertListItemDto(
                r.Id.ToString(),
                r.TreatmentSessionId.Value,
                r.AlertType.Value,
                r.Severity.Value,
                r.LifecycleState.Value,
                r.CreatedAtUtc))
            .ToList();
    }
}
