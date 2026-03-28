using MeasurementValidation.Domain.Abstractions;

using Intercessor.Abstractions;

namespace MeasurementValidation.Application.Queries.GetLatestMeasurementValidation;

public sealed record GetLatestMeasurementValidationQuery(string MeasurementId) : IQuery<MeasurementValidationSummary?>;
