using Intercessor.Abstractions;

namespace MeasurementValidation.Application.Commands.ValidateMeasurement;

public sealed record ValidateMeasurementCommand(
    Ulid CorrelationId,
    string MeasurementId,
    string ValidationProfileId,
    double? SampleValue,
    string? AuthenticatedUserId = null) : ICommand<ValidateMeasurementResult>;
