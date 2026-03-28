using Intercessor.Abstractions;

namespace QueryReadModel.Application.Queries.GetDashboardSummary;

public sealed record GetDashboardSummaryQuery : IQuery<DashboardSummaryReadDto>;
