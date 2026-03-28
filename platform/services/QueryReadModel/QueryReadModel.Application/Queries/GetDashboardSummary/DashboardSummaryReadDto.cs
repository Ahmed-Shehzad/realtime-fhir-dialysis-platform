namespace QueryReadModel.Application.Queries.GetDashboardSummary;

public sealed record DashboardSummaryReadDto(int ActiveSessionCount, int OpenAlertCount);
