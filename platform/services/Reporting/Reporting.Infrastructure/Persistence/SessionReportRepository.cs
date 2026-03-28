using Reporting.Domain;
using Reporting.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;

namespace Reporting.Infrastructure.Persistence;

public sealed class SessionReportRepository : Repository<SessionReport>, ISessionReportRepository
{
    private readonly ReportingDbContext _db;

    public SessionReportRepository(ReportingDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<SessionReport?> GetByIdAsync(Ulid reportId, CancellationToken cancellationToken = default) =>
        _db.SessionReports
            .AsSplitQuery()
            .AsNoTracking()
            .Include(r => r.Sections)
            .Include(r => r.EvidenceItems)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

    public Task<SessionReport?> GetByIdForUpdateAsync(Ulid reportId, CancellationToken cancellationToken = default) =>
        _db.SessionReports
            .AsSplitQuery()
            .Include(r => r.Sections)
            .Include(r => r.EvidenceItems)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);
}
