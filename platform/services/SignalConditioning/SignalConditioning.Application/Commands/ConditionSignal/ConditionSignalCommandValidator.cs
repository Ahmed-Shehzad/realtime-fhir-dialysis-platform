using Verifier;

namespace SignalConditioning.Application.Commands.ConditionSignal;

public sealed class ConditionSignalCommandValidator : AbstractValidator<ConditionSignalCommand>
{
    public ConditionSignalCommandValidator()
    {
        _ = RuleFor(c => c.MeasurementId).NotEmpty();
        _ = RuleFor(c => c.ChannelId).NotEmpty();
    }
}
