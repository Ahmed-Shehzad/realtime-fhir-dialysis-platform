using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;

using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetRuleSetById;

public sealed class GetRuleSetByIdQueryHandler : IQueryHandler<GetRuleSetByIdQuery, RuleSetReadDto?>
{
    private readonly IRuleSetRepository _ruleSets;

    public GetRuleSetByIdQueryHandler(IRuleSetRepository ruleSets) =>
        _ruleSets = ruleSets ?? throw new ArgumentNullException(nameof(ruleSets));

    public async Task<RuleSetReadDto?> HandleAsync(
        GetRuleSetByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        RuleSet? row = await _ruleSets.GetByIdAsync(query.RuleSetId, cancellationToken).ConfigureAwait(false);
        if (row is null)
            return null;
        return new RuleSetReadDto(
            row.Id.ToString(),
            row.Version.Value,
            row.RulesDocument.Raw,
            row.IsPublished,
            row.PublishedAtUtc);
    }
}
