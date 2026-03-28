using Verifier;

namespace MeasurementValidation.Application.Commands.ValidateMeasurement;

public sealed class ValidateMeasurementCommandValidator : AbstractValidator<ValidateMeasurementCommand>
{
    public ValidateMeasurementCommandValidator()
    {
        _ = RuleFor(c => c.MeasurementId).NotEmpty();
        _ = RuleFor(c => c.ValidationProfileId).NotEmpty();
    }
}
