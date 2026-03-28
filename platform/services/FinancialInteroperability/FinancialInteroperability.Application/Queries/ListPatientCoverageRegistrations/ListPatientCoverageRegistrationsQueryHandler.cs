using FinancialInteroperability.Domain;
using FinancialInteroperability.Domain.Abstractions;

using Intercessor.Abstractions;

namespace FinancialInteroperability.Application.Queries.ListPatientCoverageRegistrations;

public sealed class ListPatientCoverageRegistrationsQueryHandler
    : IQueryHandler<ListPatientCoverageRegistrationsQuery, IReadOnlyList<PatientCoverageRegistrationSummary>>
{
    private readonly IPatientCoverageRegistrationRepository _repository;

    public ListPatientCoverageRegistrationsQueryHandler(IPatientCoverageRegistrationRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<IReadOnlyList<PatientCoverageRegistrationSummary>> HandleAsync(
        ListPatientCoverageRegistrationsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        IReadOnlyList<PatientCoverageRegistration> rows = await _repository
            .ListByPatientIdAsync(query.PatientId, cancellationToken)
            .ConfigureAwait(false);
        return rows
            .Select(
                r => new PatientCoverageRegistrationSummary(
                    r.Id,
                    r.PatientId,
                    r.MemberIdentifier,
                    r.PayorDisplayName,
                    r.PlanDisplayName,
                    r.PeriodStart,
                    r.PeriodEnd,
                    r.FhirCoverageResourceId,
                    r.CreatedAtUtc))
            .ToList();
    }
}
