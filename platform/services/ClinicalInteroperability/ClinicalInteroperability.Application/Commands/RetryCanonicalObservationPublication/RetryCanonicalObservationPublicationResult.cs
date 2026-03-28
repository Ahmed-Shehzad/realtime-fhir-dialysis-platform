using ClinicalInteroperability.Domain;

namespace ClinicalInteroperability.Application.Commands.RetryCanonicalObservationPublication;

public sealed record RetryCanonicalObservationPublicationResult(
    bool Found,
    CanonicalPublicationState? State,
    string? ObservationId,
    string? FhirResourceReference);
