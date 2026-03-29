using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimeDelivery.Application.Abstractions;
using RealtimeDelivery.Application.Commands.BroadcastAlertFeed;
using RealtimeDelivery.Application.Commands.BroadcastSessionFeed;
using RealtimeDelivery.Domain.Contracts;

using Shouldly;

using Xunit;

namespace RealtimeDelivery.UnitTests;

public sealed class BroadcastFeedCommandHandlerTests
{
    [Fact]
    public async Task BroadcastSession_invokes_gateway_and_records_auditAsync()
    {
        var gateway = new FakeRealtimeFeedGateway();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-rd" };
        var handler = new BroadcastSessionFeedCommandHandler(gateway, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        DateTimeOffset when = DateTimeOffset.UtcNow;
        var cmd = new BroadcastSessionFeedCommand(
            correlationId,
            "session-99",
            "observation",
            "MAP snapshot",
            when,
            "user-42");

        await handler.HandleAsync(cmd);

        gateway.LastSessionTenantId.ShouldBe("tenant-rd");
        gateway.LastSessionId.ShouldBe("session-99");
        SessionFeedPayload sessionPayload = gateway.LastSessionPayload.ShouldNotBeNull();
        sessionPayload.EventType.ShouldBe("observation");
        sessionPayload.TreatmentSessionId.ShouldBe("session-99");
        sessionPayload.Summary.ShouldBe("MAP snapshot");
        sessionPayload.OccurredAtUtc.ShouldBe(when);
        sessionPayload.VitalsByChannel.ShouldBeNull();
        sessionPayload.PatientDisplayLabel.ShouldBeNull();
        sessionPayload.SessionStateHint.ShouldBeNull();
        sessionPayload.LinkedDeviceIdHint.ShouldBeNull();

        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Execute);
        r.ResourceType.ShouldBe("ClinicalFeedSession");
        r.ResourceId.ShouldBe("session-99");
        r.UserId.ShouldBe("user-42");
        r.TenantId.ShouldBe("tenant-rd");
        r.CorrelationId.ShouldBe(correlationId.ToString());
    }

    [Fact]
    public async Task BroadcastSession_forwards_optional_vitals_dictionaryAsync()
    {
        var gateway = new FakeRealtimeFeedGateway();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-rd" };
        var handler = new BroadcastSessionFeedCommandHandler(gateway, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        DateTimeOffset when = DateTimeOffset.Parse("2026-03-01T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        IReadOnlyDictionary<string, double> vitals = new Dictionary<string, double>
        {
            ["map"] = 82.5,
            ["heart-rate"] = 74,
            ["spo2"] = 97,
        };
        var cmd = new BroadcastSessionFeedCommand(
            correlationId,
            "session-100",
            "Simulation.VitalsTrend",
            "Vitals sample",
            when,
            "user-42",
            vitals);

        await handler.HandleAsync(cmd);

        SessionFeedPayload sessionPayload = gateway.LastSessionPayload.ShouldNotBeNull();
        IReadOnlyDictionary<string, double> forwarded = sessionPayload.VitalsByChannel.ShouldNotBeNull();
        forwarded["map"].ShouldBe(82.5);
        forwarded["heart-rate"].ShouldBe(74);
        forwarded["spo2"].ShouldBe(97);
    }

    [Fact]
    public async Task BroadcastSession_forwards_optional_patient_preview_hintsAsync()
    {
        var gateway = new FakeRealtimeFeedGateway();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-rd" };
        var handler = new BroadcastSessionFeedCommandHandler(gateway, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        DateTimeOffset when = DateTimeOffset.Parse("2026-03-01T12:00:00Z", System.Globalization.CultureInfo.InvariantCulture);
        var cmd = new BroadcastSessionFeedCommand(
            correlationId,
            "session-101",
            "Simulation.PatientContext",
            "Patient preview tick",
            when,
            "user-42",
            null,
            "SIM-MRN-abc123 · stream 3",
            "Monitoring",
            "dev-xyz");

        await handler.HandleAsync(cmd);

        SessionFeedPayload sessionPayload = gateway.LastSessionPayload.ShouldNotBeNull();
        sessionPayload.PatientDisplayLabel.ShouldBe("SIM-MRN-abc123 · stream 3");
        sessionPayload.SessionStateHint.ShouldBe("Monitoring");
        sessionPayload.LinkedDeviceIdHint.ShouldBe("dev-xyz");
    }

    [Fact]
    public async Task BroadcastAlert_invokes_gateway_and_records_auditAsync()
    {
        var gateway = new FakeRealtimeFeedGateway();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "t-alerts" };
        var handler = new BroadcastAlertFeedCommandHandler(gateway, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        DateTimeOffset when = new(2026, 3, 1, 12, 0, 0, TimeSpan.Zero);
        var cmd = new BroadcastAlertFeedCommand(
            correlationId,
            "raised",
            "sess-1",
            "alert-ulid",
            "high",
            "active",
            when);

        await handler.HandleAsync(cmd);

        gateway.LastAlertTenantId.ShouldBe("t-alerts");
        AlertFeedPayload alertPayload = gateway.LastAlertPayload.ShouldNotBeNull();
        alertPayload.EventType.ShouldBe("raised");
        alertPayload.TreatmentSessionId.ShouldBe("sess-1");
        alertPayload.AlertId.ShouldBe("alert-ulid");
        alertPayload.Severity.ShouldBe("high");
        alertPayload.LifecycleState.ShouldBe("active");

        audit.Records.Count.ShouldBe(1);
        audit.Records[0].ResourceType.ShouldBe("ClinicalFeedAlert");
        audit.Records[0].ResourceId.ShouldBe("alert-ulid");
    }

    [Fact]
    public async Task BroadcastAlert_allows_null_session_idAsync()
    {
        var gateway = new FakeRealtimeFeedGateway();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new BroadcastAlertFeedCommandHandler(gateway, audit, tenant);
        var cmd = new BroadcastAlertFeedCommand(
            Ulid.NewUlid(),
            "escalated",
            null,
            "a1",
            "medium",
            "escalated",
            DateTimeOffset.UtcNow);

        await handler.HandleAsync(cmd);

        AlertFeedPayload alertPayload = gateway.LastAlertPayload.ShouldNotBeNull();
        alertPayload.TreatmentSessionId.ShouldBeNull();
    }

    private sealed class FakeRealtimeFeedGateway : IRealtimeFeedGateway
    {
        public string? LastSessionTenantId { get; private set; }

        public string? LastSessionId { get; private set; }

        public SessionFeedPayload? LastSessionPayload { get; private set; }

        public string? LastAlertTenantId { get; private set; }

        public AlertFeedPayload? LastAlertPayload { get; private set; }

        public Task PushSessionAsync(
            string tenantId,
            string treatmentSessionId,
            SessionFeedPayload payload,
            CancellationToken cancellationToken = default)
        {
            LastSessionTenantId = tenantId;
            LastSessionId = treatmentSessionId;
            LastSessionPayload = payload;
            return Task.CompletedTask;
        }

        public Task PushAlertAsync(
            string tenantId,
            AlertFeedPayload payload,
            CancellationToken cancellationToken = default)
        {
            LastAlertTenantId = tenantId;
            LastAlertPayload = payload;
            return Task.CompletedTask;
        }
    }

    private sealed class CapturingAuditRecorder : IAuditRecorder
    {
        public List<AuditRecordRequest> Records { get; } = [];

        public Task RecordAsync(AuditRecordRequest request, CancellationToken cancellationToken = default)
        {
            Records.Add(request);
            return Task.CompletedTask;
        }
    }

    private sealed class StubTenantContext : ITenantContext
    {
        public string TenantId { get; init; } = "default";
    }
}
