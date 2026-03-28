namespace BuildingBlocks.Abstractions;

/// <summary>
/// Commit contract for a scoped persistence unit; typically registered via <see cref="UnitOfWork"/> over the API's <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
/// Persist uses <see cref="Interceptors.DomainEventDispatcherInterceptor"/> and
/// <see cref="Interceptors.IntegrationEventDispatcherInterceptor"/> for event dispatch and MassTransit outbox staging.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>Persists tracked changes for the current request scope.</summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}

