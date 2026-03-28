using Verifier;

namespace ClinicalInteroperability.Application.Commands.PublishCanonicalObservation;

public sealed class PublishCanonicalObservationCommandValidator : AbstractValidator<PublishCanonicalObservationCommand>
{
    public PublishCanonicalObservationCommandValidator() =>
        _ = RuleFor(c => c.MeasurementId).NotEmpty();
}
