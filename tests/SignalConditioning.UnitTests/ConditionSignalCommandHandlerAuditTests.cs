using SignalConditioning.Application.Commands.ConditionSignal;
using SignalConditioning.Domain;
using SignalConditioning.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace SignalConditioning.UnitTests;

public sealed class ConditionSignalCommandHandlerAuditTests
{
    [Fact]
    public async Task Condition_records_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakeConditioningRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-sc" };
        var handler = new ConditionSignalCommandHandler(repo, uow, audit, tenant);
        var cmd = new ConditionSignalCommand(correlationId, "m-a", "ch-1", 200d, null, "user-q");

        ConditionSignalResult result = await handler.HandleAsync(cmd);

        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Execute);
        r.ResourceType.ShouldBe("ConditioningResult");
        r.ResourceId.ShouldBe(result.ConditioningResultId.ToString());
        r.UserId.ShouldBe("user-q");
        ConditioningResult added = repo.LastAdded.ShouldNotBeNull();
        added.MeasurementId.ShouldBe("m-a");
    }

    private sealed class FakeConditioningRepository : RepositoryFakeBase<ConditioningResult>, IConditioningResultRepository
    {
        public ConditioningResult? LastAdded { get; private set; }

        public override Task AddAsync(ConditioningResult result, CancellationToken cancellationToken = default)
        {
            LastAdded = result;
            return Task.CompletedTask;
        }

        public Task<ConditioningResult?> GetLatestByMeasurementIdAsync(string measurementId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ConditioningResult?>(null);
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
