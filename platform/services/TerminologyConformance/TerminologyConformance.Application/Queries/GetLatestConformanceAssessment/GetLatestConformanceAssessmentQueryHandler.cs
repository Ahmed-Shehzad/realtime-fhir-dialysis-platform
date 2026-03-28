using TerminologyConformance.Domain;
using TerminologyConformance.Domain.Abstractions;

using Intercessor.Abstractions;

namespace TerminologyConformance.Application.Queries.GetLatestConformanceAssessment;

public sealed class GetLatestConformanceAssessmentQueryHandler
    : IQueryHandler<GetLatestConformanceAssessmentQuery, ConformanceAssessmentSummary?>
{
    private readonly IConformanceAssessmentRepository _repository;

    public GetLatestConformanceAssessmentQueryHandler(IConformanceAssessmentRepository repository) =>
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));

    public async Task<ConformanceAssessmentSummary?> HandleAsync(
        GetLatestConformanceAssessmentQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ConformanceAssessment? row = await _repository
            .GetLatestByResourceIdAsync(query.ResourceId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;
        return new ConformanceAssessmentSummary(
            row.Id,
            row.ResourceId,
            row.TerminologySliceOutcome,
            row.ProfileSliceOutcome,
            row.TerminologyReason,
            row.ProfileReason,
            row.AssessedProfileUrl,
            row.ProfileRuleRegistryVersion,
            row.EvaluatedAtUtc);
    }
}
