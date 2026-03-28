using BuildingBlocks.Abstractions;

namespace FinancialInteroperability.Domain.Abstractions;

public interface IPatientCoverageRegistrationRepository : IRepository<FinancialInteroperability.Domain.PatientCoverageRegistration>
{
    Task<FinancialInteroperability.Domain.PatientCoverageRegistration?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<FinancialInteroperability.Domain.PatientCoverageRegistration>> ListByPatientIdAsync(
        string patientId,
        CancellationToken cancellationToken = default);
}
