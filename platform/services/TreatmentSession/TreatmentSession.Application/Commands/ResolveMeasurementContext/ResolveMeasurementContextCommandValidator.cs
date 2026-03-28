using Verifier;

namespace TreatmentSession.Application.Commands.ResolveMeasurementContext;

public sealed class ResolveMeasurementContextCommandValidator : AbstractValidator<ResolveMeasurementContextCommand>
{
    public ResolveMeasurementContextCommandValidator() =>
        _ = RuleFor(c => c.MeasurementId).NotEmpty();
}
