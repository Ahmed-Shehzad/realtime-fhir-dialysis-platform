using MeasurementValidation.Domain;

namespace MeasurementValidation.Application.Commands.ValidateMeasurement;

public sealed record ValidateMeasurementResult(Ulid ValidationId, MeasurementValidationOutcome Outcome);
