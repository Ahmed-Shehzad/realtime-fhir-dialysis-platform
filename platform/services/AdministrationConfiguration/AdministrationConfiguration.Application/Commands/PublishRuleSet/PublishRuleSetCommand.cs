using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.PublishRuleSet;

public sealed record PublishRuleSetCommand(
    Ulid CorrelationId,
    Ulid RuleSetId,
    string? AuthenticatedUserId = null) : ICommand;
