using Intercessor.Abstractions;

namespace ClinicalInteroperability.Application.Commands.PublishCanonicalObservation;

public sealed record PublishCanonicalObservationCommand(
    Ulid CorrelationId,
    string MeasurementId,
    string? FhirProfileUrl,
    string? AuthenticatedUserId = null) : ICommand<PublishCanonicalObservationResult>;
