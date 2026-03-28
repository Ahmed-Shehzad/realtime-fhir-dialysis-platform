using AuditProvenance.Domain;
using AuditProvenance.Domain.Abstractions;

using BuildingBlocks;

namespace AuditProvenance.Infrastructure.Persistence;

public sealed class ProvenanceLinkRepository : Repository<ProvenanceLink>, IProvenanceLinkRepository
{
    public ProvenanceLinkRepository(AuditProvenanceDbContext db) : base(db)
    {
        ArgumentNullException.ThrowIfNull(db);
    }
}
