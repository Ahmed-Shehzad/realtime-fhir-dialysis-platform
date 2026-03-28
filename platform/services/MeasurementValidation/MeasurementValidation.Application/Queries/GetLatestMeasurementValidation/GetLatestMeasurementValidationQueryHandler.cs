using MeasurementValidation.Domain;
using MeasurementValidation.Domain.Abstractions;

using Intercessor.Abstractions;

namespace MeasurementValidation.Application.Queries.GetLatestMeasurementValidation;

public sealed class GetLatestMeasurementValidationQueryHandler : IQueryHandler<GetLatestMeasurementValidationQuery, MeasurementValidationSummary?>
{
    private readonly IMeasurementValidationRepository _repository;

    public GetLatestMeasurementValidationQueryHandler(IMeasurementValidationRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<MeasurementValidationSummary?> HandleAsync(
        GetLatestMeasurementValidationQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidatedMeasurement? row = await _repository
            .GetLatestByMeasurementIdAsync(query.MeasurementId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;
        return new MeasurementValidationSummary(
            row.Id,
            row.MeasurementId,
            row.ValidationProfileId,
            row.Outcome,
            row.Reason,
            row.RuleSetVersion,
            row.EvaluatedAtUtc);
    }
}
