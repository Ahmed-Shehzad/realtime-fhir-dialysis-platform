using RealtimePlatform.IntegrationEventCatalog;

using ClinicalInteroperability.Domain;

using Shouldly;

using Xunit;

namespace ClinicalInteroperability.UnitTests;

public sealed class CanonicalObservationPublicationRulesTests
{
    [Fact]
    public void Publish_success_emits_canonical_published_event()
    {
        Ulid c = Ulid.NewUlid();
        CanonicalObservationPublication p = CanonicalObservationPublication.StartPublication(c, "m-ok", null, null);
        p.State.ShouldBe(CanonicalPublicationState.Published);
        string href = p.FhirResourceReference.ShouldNotBeNull();
        href.ShouldContain(p.ObservationId);
        p.IntegrationEvents.OfType<CanonicalObservationPublishedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Publish_fhir_fail_stub_emits_failed_event()
    {
        Ulid c = Ulid.NewUlid();
        CanonicalObservationPublication p = CanonicalObservationPublication.StartPublication(c, "m-perm-fhir-fail", null, null);
        p.State.ShouldBe(CanonicalPublicationState.Failed);
        p.IntegrationEvents.OfType<CanonicalObservationPublicationFailedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Transient_fail_then_retry_succeeds()
    {
        Ulid c1 = Ulid.NewUlid();
        CanonicalObservationPublication p = CanonicalObservationPublication.StartPublication(
            c1,
            "m-transient-once",
            null,
            null);
        p.State.ShouldBe(CanonicalPublicationState.Failed);
        Ulid c2 = Ulid.NewUlid();
        p.RetryPublication(c2, null);
        p.State.ShouldBe(CanonicalPublicationState.Published);
        p.AttemptCount.ShouldBe(2);
        p.IntegrationEvents.OfType<CanonicalObservationPublishedIntegrationEvent>().ShouldNotBeEmpty();
    }

    [Fact]
    public void Retry_when_not_failed_throws()
    {
        Ulid c = Ulid.NewUlid();
        CanonicalObservationPublication p = CanonicalObservationPublication.StartPublication(c, "m-ok", null, null);
        _ = Should.Throw<InvalidOperationException>(() => p.RetryPublication(Ulid.NewUlid(), null));
    }

    [Fact]
    public void Transient_fail_via_fhir_profile_fragment_then_retry_succeeds()
    {
        Ulid c1 = Ulid.NewUlid();
        const string profileWithMarker = "http://hl7.org/fhir/StructureDefinition/bp#transient-once";
        CanonicalObservationPublication p = CanonicalObservationPublication.StartPublication(c1, "m-ulid-like", profileWithMarker, null);
        p.State.ShouldBe(CanonicalPublicationState.Failed);
        Ulid c2 = Ulid.NewUlid();
        p.RetryPublication(c2, null);
        p.State.ShouldBe(CanonicalPublicationState.Published);
        p.AttemptCount.ShouldBe(2);
    }

    [Fact]
    public void Transient_fail_via_fhir_profile_query_then_retry_succeeds()
    {
        Ulid c1 = Ulid.NewUlid();
        const string profileWithMarker = "http://hl7.org/fhir/StructureDefinition/bp?transient-once=1";
        CanonicalObservationPublication p = CanonicalObservationPublication.StartPublication(c1, "m-ulid-like", profileWithMarker, null);
        p.State.ShouldBe(CanonicalPublicationState.Failed);
        Ulid c2 = Ulid.NewUlid();
        p.RetryPublication(c2, null);
        p.State.ShouldBe(CanonicalPublicationState.Published);
    }
}
