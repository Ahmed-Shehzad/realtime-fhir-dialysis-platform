namespace AdministrationConfiguration.Application.Queries.GetRuleSetById;

public sealed record RuleSetReadDto(
    string Id,
    string RuleVersion,
    string RulesDocument,
    bool IsPublished,
    DateTime? PublishedAtUtc);
