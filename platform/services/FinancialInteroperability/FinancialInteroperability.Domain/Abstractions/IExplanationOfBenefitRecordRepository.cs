using BuildingBlocks.Abstractions;

namespace FinancialInteroperability.Domain.Abstractions;

public interface IExplanationOfBenefitRecordRepository : IRepository<FinancialInteroperability.Domain.ExplanationOfBenefitRecord>
{
    Task<FinancialInteroperability.Domain.ExplanationOfBenefitRecord?> GetByClaimIdAsync(Ulid claimId, CancellationToken cancellationToken = default);
}
