using Verifier;

namespace TreatmentSession.Application.Commands.MarkMeasurementUnresolved;

public sealed class MarkMeasurementContextUnresolvedCommandValidator : AbstractValidator<MarkMeasurementContextUnresolvedCommand>
{
    public MarkMeasurementContextUnresolvedCommandValidator()
    {
        _ = RuleFor(c => c.MeasurementId).NotEmpty();
        _ = RuleFor(c => c.Reason).NotEmpty();
    }
}
