using System.Linq.Expressions;

using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks;

public abstract class Repository<TEntity> : UnitOfWork, IRepository<TEntity> where TEntity : BaseEntity
{
    private readonly DbSet<TEntity> _set;

    protected Repository(DbContext dbContext) : base(dbContext)
    {
        _set = dbContext.Set<TEntity>();
    }

    public async Task<IReadOnlyList<TEntity>> GetManyAsync(Expression<Func<TEntity, bool>> expression,
        Expression<Func<TEntity, object>>? orderByExpression = null,
        bool orderByDescending = false,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TEntity> query = _set.Where(expression);

        if (orderByExpression is not null) query = orderByDescending ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression);

        List<TEntity> result = await query.ToListAsync(cancellationToken);

        return result.AsReadOnly();
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> expression,
        CancellationToken cancellationToken = default) => _set.FirstOrDefaultAsync(expression, cancellationToken);

    public async Task AddAsync(TEntity entity,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = await _set.AddAsync(entity, cancellationToken);
    }

    public async Task AddAsync(IEnumerable<TEntity> entities,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(entities);
        await _set.AddRangeAsync(entities, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = _set.Update(entity);
    }

    public void Update(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _set.UpdateRange(entities);
    }

    public void Delete(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ = _set.Remove(entity);
    }

    public void Delete(IEnumerable<TEntity> entities)
    {
        ArgumentNullException.ThrowIfNull(entities);
        _set.RemoveRange(entities);
    }
}
