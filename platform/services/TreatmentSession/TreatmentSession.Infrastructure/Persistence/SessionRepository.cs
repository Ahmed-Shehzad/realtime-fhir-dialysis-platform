using Microsoft.EntityFrameworkCore;

using TreatmentSession.Domain;
using TreatmentSession.Domain.Abstractions;

using BuildingBlocks;

namespace TreatmentSession.Infrastructure.Persistence;

public sealed class SessionRepository : Repository<DialysisSession>, ISessionRepository
{
    private readonly TreatmentSessionDbContext _db;

    public SessionRepository(TreatmentSessionDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    /// <inheritdoc />
    public Task<DialysisSession?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
        _db.DialysisSessions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
}
