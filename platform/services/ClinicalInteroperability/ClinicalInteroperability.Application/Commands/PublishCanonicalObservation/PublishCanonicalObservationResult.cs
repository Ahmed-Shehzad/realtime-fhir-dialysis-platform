using ClinicalInteroperability.Domain;

namespace ClinicalInteroperability.Application.Commands.PublishCanonicalObservation;

public sealed record PublishCanonicalObservationResult(
    Ulid PublicationId,
    CanonicalPublicationState State,
    string ObservationId,
    string? FhirResourceReference);
