using System.Linq.Expressions;

using BuildingBlocks;
using BuildingBlocks.Abstractions;

namespace RealtimePlatform.UnitTesting;

/// <summary>
/// Default no-op implementations for <see cref="IRepository{TEntity}"/> members in unit test fakes.
/// </summary>
public abstract class RepositoryFakeBase<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
{
    public virtual Task<IReadOnlyList<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> expression,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default) =>
        Task.FromResult<IReadOnlyList<TEntity>>([]);

    public virtual Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression, CancellationToken cancellationToken = default) =>
        Task.FromResult<TEntity?>(null);

    public virtual Task AddAsync(TEntity entity, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public virtual Task AddAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default) =>
        Task.CompletedTask;

    public virtual void Update(TEntity entity)
    {
    }

    public virtual void Update(IEnumerable<TEntity> entities)
    {
    }

    public virtual void Delete(TEntity entity)
    {
    }

    public virtual void Delete(IEnumerable<TEntity> entities)
    {
    }

    public virtual Task<int> CommitAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(0);
}
