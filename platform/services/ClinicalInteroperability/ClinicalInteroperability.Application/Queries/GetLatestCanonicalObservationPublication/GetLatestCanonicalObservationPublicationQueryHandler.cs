using ClinicalInteroperability.Domain;
using ClinicalInteroperability.Domain.Abstractions;

using Intercessor.Abstractions;

namespace ClinicalInteroperability.Application.Queries.GetLatestCanonicalObservationPublication;

public sealed class GetLatestCanonicalObservationPublicationQueryHandler
    : IQueryHandler<GetLatestCanonicalObservationPublicationQuery, CanonicalObservationPublicationSummary?>
{
    private readonly ICanonicalObservationPublicationRepository _repository;

    public GetLatestCanonicalObservationPublicationQueryHandler(ICanonicalObservationPublicationRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<CanonicalObservationPublicationSummary?> HandleAsync(
        GetLatestCanonicalObservationPublicationQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        CanonicalObservationPublication? row = await _repository
            .GetLatestByMeasurementIdAsync(query.MeasurementId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;
        return new CanonicalObservationPublicationSummary(
            row.Id,
            row.MeasurementId,
            row.ObservationId,
            row.State,
            row.FhirResourceReference,
            row.LastFailureReason,
            row.AttemptCount,
            row.LastAttemptAtUtc);
    }
}
