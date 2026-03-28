using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;
using BuildingBlocks.Tenancy;

namespace FinancialInteroperability.Infrastructure.Persistence;

public sealed class DialysisFinancialClaimRepository : Repository<DialysisFinancialClaim>, IDialysisFinancialClaimRepository
{
    private readonly FinancialInteroperabilityDbContext _db;
    private readonly ITenantContext _tenant;

    public DialysisFinancialClaimRepository(FinancialInteroperabilityDbContext db, ITenantContext tenant) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<DialysisFinancialClaim?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
        await _db.DialysisFinancialClaims
            .FirstOrDefaultAsync(c => c.Id == id && c.TenantId == _tenant.TenantId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<DialysisFinancialClaim?> GetLatestByTreatmentSessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return null;
        string sid = treatmentSessionId.Trim();
        return await _db.DialysisFinancialClaims
            .AsNoTracking()
            .Where(c => c.TreatmentSessionId == sid && c.TenantId == _tenant.TenantId)
            .OrderByDescending(c => c.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<DialysisFinancialClaim>> ListByTreatmentSessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return Array.Empty<DialysisFinancialClaim>();
        string sid = treatmentSessionId.Trim();
        return await _db.DialysisFinancialClaims
            .AsNoTracking()
            .Where(c => c.TreatmentSessionId == sid && c.TenantId == _tenant.TenantId)
            .OrderBy(c => c.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
