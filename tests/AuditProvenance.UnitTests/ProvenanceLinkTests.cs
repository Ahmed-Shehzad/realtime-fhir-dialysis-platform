using AuditProvenance.Domain;

using Shouldly;

using Xunit;

namespace AuditProvenance.UnitTests;

public sealed class ProvenanceLinkTests
{
    [Fact]
    public void Create_same_from_and_to_throws()
    {
        Ulid id = Ulid.NewUlid();
        _ = Should.Throw<InvalidOperationException>(() =>
            ProvenanceLink.Create(Ulid.NewUlid(), id, id, "derivesFrom", null));
    }
}
