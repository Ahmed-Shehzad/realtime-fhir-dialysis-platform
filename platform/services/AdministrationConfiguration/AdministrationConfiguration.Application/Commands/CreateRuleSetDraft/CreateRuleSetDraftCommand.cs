using Intercessor.Abstractions;

namespace AdministrationConfiguration.Application.Commands.CreateRuleSetDraft;

public sealed record CreateRuleSetDraftCommand(
    Ulid CorrelationId,
    string RuleVersion,
    string RulesDocument,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
