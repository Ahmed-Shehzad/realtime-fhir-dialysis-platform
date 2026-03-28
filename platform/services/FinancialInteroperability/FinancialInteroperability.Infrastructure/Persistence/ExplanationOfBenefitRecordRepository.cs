using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;
using BuildingBlocks.Tenancy;

namespace FinancialInteroperability.Infrastructure.Persistence;

public sealed class ExplanationOfBenefitRecordRepository : Repository<ExplanationOfBenefitRecord>, IExplanationOfBenefitRecordRepository
{
    private readonly FinancialInteroperabilityDbContext _db;
    private readonly ITenantContext _tenant;

    public ExplanationOfBenefitRecordRepository(FinancialInteroperabilityDbContext db, ITenantContext tenant) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<ExplanationOfBenefitRecord?> GetByClaimIdAsync(Ulid claimId, CancellationToken cancellationToken = default) =>
        await _db.ExplanationOfBenefitRecords
            .AsNoTracking()
            .FirstOrDefaultAsync(
                e => e.DialysisFinancialClaimId == claimId && e.TenantId == _tenant.TenantId,
                cancellationToken)
            .ConfigureAwait(false);
}
