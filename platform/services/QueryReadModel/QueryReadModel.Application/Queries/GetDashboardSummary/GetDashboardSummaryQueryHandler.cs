using QueryReadModel.Domain.Abstractions;

using Intercessor.Abstractions;

namespace QueryReadModel.Application.Queries.GetDashboardSummary;

public sealed class GetDashboardSummaryQueryHandler : IQueryHandler<GetDashboardSummaryQuery, DashboardSummaryReadDto>
{
    private readonly IDashboardReadModelQuery _dashboard;
    private readonly IAlertProjectionRepository _alerts;

    public GetDashboardSummaryQueryHandler(
        IDashboardReadModelQuery dashboard,
        IAlertProjectionRepository alerts)
    {
        _dashboard = dashboard ?? throw new ArgumentNullException(nameof(dashboard));
        _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));
    }

    public async Task<DashboardSummaryReadDto> HandleAsync(
        GetDashboardSummaryQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        int sessions = await _dashboard.CountActiveSessionsAsync(cancellationToken).ConfigureAwait(false);
        int alerts = await _alerts.CountOpenAsync(cancellationToken).ConfigureAwait(false);
        return new DashboardSummaryReadDto(sessions, alerts);
    }
}
