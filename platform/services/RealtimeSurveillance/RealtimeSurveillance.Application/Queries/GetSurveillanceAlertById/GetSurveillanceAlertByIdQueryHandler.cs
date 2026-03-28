using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Queries.GetSurveillanceAlertById;

public sealed class GetSurveillanceAlertByIdQueryHandler : IQueryHandler<GetSurveillanceAlertByIdQuery, SurveillanceAlertReadDto?>
{
    private readonly ISurveillanceAlertRepository _alerts;

    public GetSurveillanceAlertByIdQueryHandler(ISurveillanceAlertRepository alerts) =>
        _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));

    public async Task<SurveillanceAlertReadDto?> HandleAsync(
        GetSurveillanceAlertByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        SurveillanceAlert? row = await _alerts.GetByIdAsync(query.AlertId, cancellationToken).ConfigureAwait(false);
        if (row is null)
            return null;
        return new SurveillanceAlertReadDto(
            row.Id.ToString(),
            row.TreatmentSessionId.Value,
            row.AlertType.Value,
            row.Severity.Value,
            row.LifecycleState.Value,
            row.Detail,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
