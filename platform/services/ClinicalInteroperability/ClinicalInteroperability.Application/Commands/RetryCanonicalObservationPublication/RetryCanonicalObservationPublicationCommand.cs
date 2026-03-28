using Intercessor.Abstractions;

namespace ClinicalInteroperability.Application.Commands.RetryCanonicalObservationPublication;

public sealed record RetryCanonicalObservationPublicationCommand(
    Ulid CorrelationId,
    Ulid PublicationId,
    string? AuthenticatedUserId = null) : ICommand<RetryCanonicalObservationPublicationResult>;
