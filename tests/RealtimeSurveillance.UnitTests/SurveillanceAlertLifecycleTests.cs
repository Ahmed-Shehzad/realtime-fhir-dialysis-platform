using BuildingBlocks.ValueObjects;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.ValueObjects;

using Shouldly;

using Xunit;

namespace RealtimeSurveillance.UnitTests;

public sealed class SurveillanceAlertLifecycleTests
{
    [Fact]
    public void Raise_starts_activeAsync()
    {
        Ulid corr = Ulid.NewUlid();
        var sessionId = new SessionId("sess-1");
        var alert = SurveillanceAlert.Raise(
            corr,
            sessionId,
            new AlertTypeCode("HYPOTENSION"),
            new AlertSeverityLevel("High"),
            "detail",
            null);

        alert.LifecycleState.ShouldBe(AlertLifecycleState.Active);
    }

    [Fact]
    public void Acknowledge_moves_to_acknowledgedAsync()
    {
        SurveillanceAlert alert = CreateActiveAlert();
        alert.Acknowledge(Ulid.NewUlid(), "nurse-1", null);
        alert.LifecycleState.ShouldBe(AlertLifecycleState.Acknowledged);
    }

    [Fact]
    public void Second_acknowledge_throwsAsync()
    {
        SurveillanceAlert alert = CreateActiveAlert();
        alert.Acknowledge(Ulid.NewUlid(), "nurse-1", null);
        _ = Should.Throw<InvalidOperationException>(() => alert.Acknowledge(Ulid.NewUlid(), "nurse-2", null));
    }

    [Fact]
    public void Resolve_from_active_succeedsAsync()
    {
        SurveillanceAlert alert = CreateActiveAlert();
        alert.Resolve(Ulid.NewUlid(), "recovered", null);
        alert.LifecycleState.ShouldBe(AlertLifecycleState.Resolved);
    }

    private static SurveillanceAlert CreateActiveAlert() =>
        SurveillanceAlert.Raise(
            Ulid.NewUlid(),
            new SessionId("sess-1"),
            new AlertTypeCode("TEST"),
            new AlertSeverityLevel("Low"),
            null,
            null);
}
