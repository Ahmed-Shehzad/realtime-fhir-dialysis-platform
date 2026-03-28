using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;
using BuildingBlocks.Tenancy;

namespace FinancialInteroperability.Infrastructure.Persistence;

public sealed class CoverageEligibilityInquiryRepository : Repository<CoverageEligibilityInquiry>, ICoverageEligibilityInquiryRepository
{
    private readonly FinancialInteroperabilityDbContext _db;
    private readonly ITenantContext _tenant;

    public CoverageEligibilityInquiryRepository(FinancialInteroperabilityDbContext db, ITenantContext tenant) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<CoverageEligibilityInquiry?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
        await _db.CoverageEligibilityInquiries
            .AsNoTracking()
            .FirstOrDefaultAsync(i => i.Id == id && i.TenantId == _tenant.TenantId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<CoverageEligibilityInquiry>> ListByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return Array.Empty<CoverageEligibilityInquiry>();
        string pid = patientId.Trim();
        return await _db.CoverageEligibilityInquiries
            .AsNoTracking()
            .Where(i => i.PatientId == pid && i.TenantId == _tenant.TenantId)
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
