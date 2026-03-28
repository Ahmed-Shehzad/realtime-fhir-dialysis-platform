namespace ClinicalInteroperability.Domain.Abstractions;

public sealed record CanonicalObservationPublicationSummary(
    Ulid Id,
    string MeasurementId,
    string ObservationId,
    global::ClinicalInteroperability.Domain.CanonicalPublicationState State,
    string? FhirResourceReference,
    string? LastFailureReason,
    int AttemptCount,
    DateTimeOffset LastAttemptAtUtc);
