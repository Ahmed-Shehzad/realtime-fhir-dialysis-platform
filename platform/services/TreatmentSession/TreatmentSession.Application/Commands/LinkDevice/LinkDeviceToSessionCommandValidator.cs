using Verifier;

namespace TreatmentSession.Application.Commands.LinkDevice;

public sealed class LinkDeviceToSessionCommandValidator : AbstractValidator<LinkDeviceToSessionCommand>
{
    public LinkDeviceToSessionCommandValidator() =>
        _ = RuleFor(c => c.DeviceIdentifier).NotEmpty();
}
