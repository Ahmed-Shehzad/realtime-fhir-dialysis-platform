using FinancialInteroperability.Domain.Abstractions;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Queries.ListPatientCoverageRegistrations;

public sealed record ListPatientCoverageRegistrationsQuery(string PatientId)
    : IQuery<IReadOnlyList<PatientCoverageRegistrationSummary>>;
