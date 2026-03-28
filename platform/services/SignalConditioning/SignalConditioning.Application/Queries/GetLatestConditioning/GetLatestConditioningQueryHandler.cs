using SignalConditioning.Domain;
using SignalConditioning.Domain.Abstractions;

using Intercessor.Abstractions;

namespace SignalConditioning.Application.Queries.GetLatestConditioning;

public sealed class GetLatestConditioningQueryHandler : IQueryHandler<GetLatestConditioningQuery, ConditioningSummary?>
{
    private readonly IConditioningResultRepository _repository;

    public GetLatestConditioningQueryHandler(IConditioningResultRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<ConditioningSummary?> HandleAsync(
        GetLatestConditioningQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ConditioningResult? row = await _repository
            .GetLatestByMeasurementIdAsync(query.MeasurementId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;
        return new ConditioningSummary(
            row.Id,
            row.MeasurementId,
            row.ChannelId,
            row.IsDropout,
            row.DriftDetected,
            row.QualityScorePercent,
            row.ConditioningMethodVersion,
            row.ConditionedSignalKind,
            row.EvaluatedAtUtc);
    }
}
