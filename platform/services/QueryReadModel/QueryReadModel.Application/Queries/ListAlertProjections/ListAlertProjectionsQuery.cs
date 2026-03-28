using Intercessor.Abstractions;

namespace QueryReadModel.Application.Queries.ListAlertProjections;

public sealed record ListAlertProjectionsQuery(string? SeverityFilter = null)
    : IQuery<IReadOnlyList<AlertProjectionReadDto>>;
