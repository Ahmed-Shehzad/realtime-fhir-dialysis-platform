using DeviceRegistry.Domain;

using Verifier;

namespace DeviceRegistry.Application.Commands.RegisterDevice;

/// <summary>
/// Validation for <see cref="RegisterDeviceCommand"/>.
/// </summary>
public sealed class RegisterDeviceCommandValidator : AbstractValidator<RegisterDeviceCommand>
{
    public RegisterDeviceCommandValidator()
    {
        _ = RuleFor(c => c.DeviceIdentifier).NotEmpty();
        _ = RuleFor(c => c.InitialTrustState).NotEmpty();
        _ = RuleFor(c => c.Manufacturer)
            .Must(
                m => m is null || m.Length <= Device.MaxManufacturerLength,
                $"Manufacturer must be at most {Device.MaxManufacturerLength} characters.");
    }
}
