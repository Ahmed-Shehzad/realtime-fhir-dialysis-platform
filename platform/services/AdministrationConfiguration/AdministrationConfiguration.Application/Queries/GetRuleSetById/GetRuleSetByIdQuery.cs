using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Queries.GetRuleSetById;

public sealed record GetRuleSetByIdQuery(Ulid RuleSetId) : IQuery<RuleSetReadDto?>;
