using BuildingBlocks.Abstractions;

namespace FinancialInteroperability.Domain.Abstractions;

public interface ICoverageEligibilityInquiryRepository : IRepository<FinancialInteroperability.Domain.CoverageEligibilityInquiry>
{
    Task<FinancialInteroperability.Domain.CoverageEligibilityInquiry?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialInteroperability.Domain.CoverageEligibilityInquiry>> ListByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken = default);
}
