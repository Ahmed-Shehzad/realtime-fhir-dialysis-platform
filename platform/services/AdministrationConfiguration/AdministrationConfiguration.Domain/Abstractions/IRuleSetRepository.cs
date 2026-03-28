using BuildingBlocks.Abstractions;

namespace AdministrationConfiguration.Domain.Abstractions;

public interface IRuleSetRepository : IRepository<RuleSet>
{
    Task<RuleSet?> GetByIdAsync(Ulid ruleSetId, CancellationToken cancellationToken = default);

    Task<RuleSet?> GetByIdForUpdateAsync(Ulid ruleSetId, CancellationToken cancellationToken = default);
}
