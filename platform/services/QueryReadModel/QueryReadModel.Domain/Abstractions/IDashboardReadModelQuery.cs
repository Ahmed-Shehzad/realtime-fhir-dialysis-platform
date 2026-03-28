namespace QueryReadModel.Domain.Abstractions;

public interface IDashboardReadModelQuery
{
    Task<int> CountActiveSessionsAsync(CancellationToken cancellationToken = default);
}
