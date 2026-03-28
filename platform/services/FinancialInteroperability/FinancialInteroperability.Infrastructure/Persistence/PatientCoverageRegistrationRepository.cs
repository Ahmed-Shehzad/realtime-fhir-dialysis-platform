using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using Microsoft.EntityFrameworkCore;

using BuildingBlocks;
using BuildingBlocks.Tenancy;

namespace FinancialInteroperability.Infrastructure.Persistence;

public sealed class PatientCoverageRegistrationRepository : Repository<PatientCoverageRegistration>, IPatientCoverageRegistrationRepository
{
    private readonly FinancialInteroperabilityDbContext _db;
    private readonly ITenantContext _tenant;

    public PatientCoverageRegistrationRepository(FinancialInteroperabilityDbContext db, ITenantContext tenant) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<PatientCoverageRegistration?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
        await _db.PatientCoverageRegistrations
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && r.TenantId == _tenant.TenantId, cancellationToken)
            .ConfigureAwait(false);

    public async Task<IReadOnlyList<PatientCoverageRegistration>> ListByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(patientId))
            return Array.Empty<PatientCoverageRegistration>();
        string pid = patientId.Trim();
        return await _db.PatientCoverageRegistrations
            .AsNoTracking()
            .Where(r => r.PatientId == pid && r.TenantId == _tenant.TenantId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }
}
