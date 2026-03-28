using BuildingBlocks.Abstractions;

namespace FinancialInteroperability.Domain.Abstractions;

public interface IDialysisFinancialClaimRepository : IRepository<FinancialInteroperability.Domain.DialysisFinancialClaim>
{
    Task<FinancialInteroperability.Domain.DialysisFinancialClaim?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default);

    Task<FinancialInteroperability.Domain.DialysisFinancialClaim?> GetLatestByTreatmentSessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialInteroperability.Domain.DialysisFinancialClaim>> ListByTreatmentSessionIdAsync(
        string treatmentSessionId,
        CancellationToken cancellationToken = default);
}
