using BuildingBlocks;

using AdministrationConfiguration.Domain.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

namespace AdministrationConfiguration.Domain;

public sealed class RuleSet : AggregateRoot
{
    private RuleSet()
    {
    }

    public RuleVersion Version { get; private set; } = null!;

    public RulesDocumentPayload RulesDocument { get; private set; }

    public bool IsPublished { get; private set; }

    public DateTime? PublishedAtUtc { get; private set; }

    public static RuleSet CreateDraft(RuleVersion version, RulesDocumentPayload rulesDocument)
    {
        var set = new RuleSet
        {
            Version = version,
            RulesDocument = rulesDocument,
            IsPublished = false,
            PublishedAtUtc = null,
        };
        set.ApplyCreatedDateTime();
        return set;
    }

    public void ReplaceDraftDocument(RulesDocumentPayload rulesDocument)
    {
        if (IsPublished)
            throw new InvalidOperationException("Cannot change rules on a published rule set.");
        RulesDocument = rulesDocument;
        ApplyUpdateDateTime();
    }

    public void Publish(Ulid correlationId, string? tenantId)
    {
        if (IsPublished)
            throw new InvalidOperationException("Rule set is already published.");
        RulesDocument.EnsurePublishable();
        IsPublished = true;
        PublishedAtUtc = DateTime.UtcNow;
        ApplyUpdateDateTime();
        ApplyEvent(
            new RuleSetPublishedIntegrationEvent(correlationId, Id.ToString(), Version.Value)
            {
                TenantId = tenantId,
            });
    }
}
