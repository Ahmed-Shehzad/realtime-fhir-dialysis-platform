using AdministrationConfiguration.Domain;
using AdministrationConfiguration.Domain.ValueObjects;

using Shouldly;

using Xunit;

namespace AdministrationConfiguration.UnitTests;

public sealed class RuleSetPublishValidationTests
{
    [Fact]
    public void Publish_rejects_empty_document()
    {
        var version = new RuleVersion("1.0.0");
        var draft = new RulesDocumentPayload("   ");
        RuleSet set = RuleSet.CreateDraft(version, draft);
        InvalidOperationException ex = Should.Throw<InvalidOperationException>(() => set.Publish(Ulid.NewUlid(), null));
        ex.Message.ShouldContain("empty");
    }

    [Fact]
    public void Publish_accepts_valid_json_object()
    {
        var version = new RuleVersion("1.0.0");
        var draft = new RulesDocumentPayload("""{"rules":[]}""");
        RuleSet set = RuleSet.CreateDraft(version, draft);
        set.Publish(Ulid.NewUlid(), null);
        set.IsPublished.ShouldBeTrue();
    }
}
