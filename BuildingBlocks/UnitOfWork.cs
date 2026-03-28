using BuildingBlocks.Abstractions;

using Microsoft.EntityFrameworkCore;

namespace BuildingBlocks;

public class UnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;

    protected UnitOfWork(DbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync(CancellationToken cancellationToken = default) => _context.SaveChangesAsync(cancellationToken);
}
