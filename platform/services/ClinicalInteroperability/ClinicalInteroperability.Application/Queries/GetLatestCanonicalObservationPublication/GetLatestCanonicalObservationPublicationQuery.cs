using Intercessor.Abstractions;

using ClinicalInteroperability.Domain.Abstractions;

namespace ClinicalInteroperability.Application.Queries.GetLatestCanonicalObservationPublication;

public sealed record GetLatestCanonicalObservationPublicationQuery(string MeasurementId)
    : IQuery<CanonicalObservationPublicationSummary?>;
