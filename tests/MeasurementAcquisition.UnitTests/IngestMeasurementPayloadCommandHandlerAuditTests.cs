using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using MeasurementAcquisition.Application.Commands.IngestMeasurement;
using MeasurementAcquisition.Domain;
using MeasurementAcquisition.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace MeasurementAcquisition.UnitTests;

public sealed class IngestMeasurementPayloadCommandHandlerAuditTests
{
    [Fact]
    public async Task Accepted_payload_records_audit_for_envelopeAsync()
    {
        var correlationId = Ulid.NewUlid();
        const string deviceId = "ma-audit-dev";
        var repo = new FakeAcquisitionRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-ma" };
        var handler = new IngestMeasurementPayloadCommandHandler(repo, uow, audit, tenant);
        var cmd = new IngestMeasurementPayloadCommand(
            correlationId,
            deviceId,
            Channel: "ch1",
            MeasurementType: "vitals",
            SchemaVersion: "1",
            RawPayloadJson: """{"ok":true}""",
            AuthenticatedUserId: "sub-1");

        IngestMeasurementPayloadResult result = await handler.HandleAsync(cmd);

        result.Status.ShouldBe(AcquisitionStatus.Accepted);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Create);
        r.ResourceType.ShouldBe("RawMeasurementEnvelope");
        r.ResourceId.ShouldBe(result.MeasurementId);
        r.UserId.ShouldBe("sub-1");
        r.Outcome.ShouldBe(AuditOutcome.Success);
        r.TenantId.ShouldBe("tenant-ma");
        r.CorrelationId.ShouldBe(correlationId.ToString());
        string acceptedDescription = r.Description ?? throw new InvalidOperationException("Expected audit description.");
        acceptedDescription.ShouldContain("accepted", Case.Insensitive);
    }

    [Fact]
    public async Task Rejected_payload_still_records_success_outcome_audit_with_rejection_in_descriptionAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakeAcquisitionRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new IngestMeasurementPayloadCommandHandler(repo, uow, audit, tenant);
        var cmd = new IngestMeasurementPayloadCommand(
            correlationId,
            "dev-bad-json",
            "ch",
            "t",
            "1",
            RawPayloadJson: "{ not json",
            AuthenticatedUserId: null);

        IngestMeasurementPayloadResult result = await handler.HandleAsync(cmd);

        result.Status.ShouldBe(AcquisitionStatus.Rejected);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Outcome.ShouldBe(AuditOutcome.Success);
        string rejectedDescription = r.Description ?? throw new InvalidOperationException("Expected audit description.");
        rejectedDescription.ShouldContain("rejected", Case.Insensitive);
    }

    private sealed class FakeAcquisitionRepository : RepositoryFakeBase<RawMeasurementEnvelope>, IAcquisitionRepository
    {
        public List<RawMeasurementEnvelope> Added { get; } = [];

        public override Task AddAsync(RawMeasurementEnvelope envelope, CancellationToken cancellationToken = default)
        {
            Added.Add(envelope);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> CommitAsync(CancellationToken cancellationToken = default)
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
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
