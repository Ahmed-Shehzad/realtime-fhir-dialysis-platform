using AuditProvenance.Domain.Abstractions;

using Intercessor.Abstractions;

namespace AuditProvenance.Application.Queries.GetRecentPlatformAuditFacts;

public sealed class GetRecentPlatformAuditFactsQueryHandler : IQueryHandler<GetRecentPlatformAuditFactsQuery, IReadOnlyList<PlatformAuditFactSummary>>
{
    private readonly IPlatformAuditFactRepository _facts;

    public GetRecentPlatformAuditFactsQueryHandler(IPlatformAuditFactRepository facts) =>
        _facts = facts ?? throw new ArgumentNullException(nameof(facts));

    public Task<IReadOnlyList<PlatformAuditFactSummary>> HandleAsync(
        GetRecentPlatformAuditFactsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _facts.GetRecentSummariesAsync(query.Count, cancellationToken);
    }
}
