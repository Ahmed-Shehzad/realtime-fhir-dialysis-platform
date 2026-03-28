using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using BuildingBlocks;

namespace QueryReadModel.Infrastructure.Persistence;

public sealed class ReadModelRebuildRecordRepository : Repository<ReadModelRebuildRecord>, IReadModelRebuildRecordRepository
{
    public ReadModelRebuildRecordRepository(QueryReadModelDbContext db) : base(db)
    {
        ArgumentNullException.ThrowIfNull(db);
    }
}
