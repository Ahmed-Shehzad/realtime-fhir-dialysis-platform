using BuildingBlocks.ValueObjects;

using TreatmentSession.Domain;

using Shouldly;

using Xunit;

namespace TreatmentSession.UnitTests;

public sealed class DialysisSessionLifecycleTests
{
    [Fact]
    public void Create_is_Created_and_raises_created_integration_event()
    {
        Ulid correlationId = Ulid.NewUlid();
        DialysisSession session = DialysisSession.Create(correlationId, "t1");

        session.State.ShouldBe(DialysisSessionLifecycleState.Created);
        session.IntegrationEvents.ShouldNotBeEmpty();
    }

    [Fact]
    public void Start_without_patient_throws()
    {
        DialysisSession session = DialysisSession.Create(Ulid.NewUlid(), null);
        session.LinkDevice(new DeviceId("dev-1"));
        _ = Should.Throw<InvalidOperationException>(() => session.Start(Ulid.NewUlid(), null));
    }

    [Fact]
    public void Start_after_assign_and_link_makes_Active()
    {
        Ulid c = Ulid.NewUlid();
        DialysisSession session = DialysisSession.Create(c, "t");
        session.AssignPatient(c, new MedicalRecordNumber("MRN-1"), "t");
        session.LinkDevice(new DeviceId("dev-2"));
        session.Start(Ulid.NewUlid(), "t");

        session.State.ShouldBe(DialysisSessionLifecycleState.Active);
    }

    [Fact]
    public void Complete_from_Created_throws()
    {
        DialysisSession session = DialysisSession.Create(Ulid.NewUlid(), null);
        _ = Should.Throw<InvalidOperationException>(() => session.Complete(Ulid.NewUlid(), null));
    }

    [Fact]
    public void Resolve_context_when_not_active_throws()
    {
        DialysisSession session = DialysisSession.Create(Ulid.NewUlid(), null);
        _ = Should.Throw<InvalidOperationException>(() => session.ResolveMeasurementContext(Ulid.NewUlid(), "m1", null));
    }
}
