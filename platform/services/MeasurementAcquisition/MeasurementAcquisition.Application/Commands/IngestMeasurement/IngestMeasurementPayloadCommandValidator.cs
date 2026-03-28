using MeasurementAcquisition.Domain;

using Verifier;

namespace MeasurementAcquisition.Application.Commands.IngestMeasurement;

/// <summary>
/// Validates <see cref="IngestMeasurementPayloadCommand"/>.
/// </summary>
public sealed class IngestMeasurementPayloadCommandValidator : AbstractValidator<IngestMeasurementPayloadCommand>
{
    public const int MaxPayloadLength = 512 * 1024;

    public IngestMeasurementPayloadCommandValidator()
    {
        _ = RuleFor(c => c.DeviceIdentifier).NotEmpty();
        _ = RuleFor(c => c.Channel).NotEmpty();
        _ = RuleFor(c => c.Channel)
            .Must(c => c.Length <= RawMeasurementEnvelope.MaxChannelLength, $"Channel must be at most {RawMeasurementEnvelope.MaxChannelLength} characters.");
        _ = RuleFor(c => c.MeasurementType).NotEmpty();
        _ = RuleFor(c => c.MeasurementType)
            .Must(
                t => t.Length <= RawMeasurementEnvelope.MaxMeasurementTypeLength,
                $"Measurement type must be at most {RawMeasurementEnvelope.MaxMeasurementTypeLength} characters.");
        _ = RuleFor(c => c.SchemaVersion).NotEmpty();
        _ = RuleFor(c => c.SchemaVersion)
            .Must(
                s => s.Length <= RawMeasurementEnvelope.MaxSchemaVersionLength,
                $"Schema version must be at most {RawMeasurementEnvelope.MaxSchemaVersionLength} characters.");
        _ = RuleFor(c => c.RawPayloadJson).NotEmpty();
        _ = RuleFor(c => c.RawPayloadJson)
            .Must(j => j.Length <= MaxPayloadLength, $"Payload must be at most {MaxPayloadLength} characters.");
    }
}
