using AuditProvenance.Domain;

using RealtimePlatform.IntegrationEventCatalog;

using Shouldly;

using Xunit;

namespace AuditProvenance.UnitTests;

public sealed class PlatformAuditFactTests
{
    [Fact]
    public void Record_raises_critical_audit_integration_event()
    {
        Ulid c = Ulid.NewUlid();
        PlatformAuditFact fact = PlatformAuditFact.Record(
            c,
            new PlatformAuditFactPayload
            {
                OccurredAtUtc = DateTimeOffset.UtcNow,
                EventType = "Security.FhirWrite",
                Summary = "Observation created",
                DetailJson = null,
                CorrelationIdString = null,
                CausationIdString = null,
                TenantId = "tenant-a",
                ActorId = "actor-1",
                SourceSystem = "TreatmentSession.Api",
                RelatedResourceType = "Observation",
                RelatedResourceId = "obs-99",
                SessionId = null,
                PatientId = null,
            });

        fact.IntegrationEvents.Count.ShouldBe(1);
        CriticalAuditEventRecordedIntegrationEvent evt = fact.IntegrationEvents
            .OfType<CriticalAuditEventRecordedIntegrationEvent>()
            .Single();
        evt.PlatformAuditFactId.ShouldBe(fact.Id.ToString());
        evt.EventType.ShouldBe("Security.FhirWrite");
        evt.TenantId.ShouldBe("tenant-a");
    }
}
