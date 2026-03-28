using Microsoft.EntityFrameworkCore;

using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.Abstractions;

using BuildingBlocks;

namespace AdministrationConfiguration.Infrastructure.Persistence;

public sealed class RuleSetRepository : Repository<RuleSet>, IRuleSetRepository
{
    private readonly AdministrationConfigurationDbContext _db;

    public RuleSetRepository(AdministrationConfigurationDbContext db) : base(db)
    {
        _db = db ?? throw new ArgumentNullException(nameof(db));
    }

    public Task<RuleSet?> GetByIdAsync(Ulid ruleSetId, CancellationToken cancellationToken = default) =>
        _db.RuleSets.AsNoTracking().FirstOrDefaultAsync(r => r.Id == ruleSetId, cancellationToken);

    public Task<RuleSet?> GetByIdForUpdateAsync(Ulid ruleSetId, CancellationToken cancellationToken = default) =>
        _db.RuleSets.FirstOrDefaultAsync(r => r.Id == ruleSetId, cancellationToken);
}
