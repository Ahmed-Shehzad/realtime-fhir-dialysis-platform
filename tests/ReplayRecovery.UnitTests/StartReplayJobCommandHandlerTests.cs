using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using ReplayRecovery.Application.Commands.StartReplayJob;
using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace ReplayRecovery.UnitTests;

public sealed class StartReplayJobCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_persists_job_and_records_auditAsync()
    {
        var repo = new FakeReplayJobRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-rr" };
        var handler = new StartReplayJobCommandHandler(repo, uow, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        var cmd = new StartReplayJobCommand(correlationId, "Deterministic", "session-overview", "user-a");

        Ulid jobId = await handler.HandleAsync(cmd);

        ReplayJob added = repo.LastAdded.ShouldNotBeNull();
        jobId.ShouldBe(added.Id);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        audit.Records[0].ResourceType.ShouldBe("ReplayJob");
        audit.Records[0].TenantId.ShouldBe("tenant-rr");
        added.IntegrationEvents.Count.ShouldBe(1);
    }

    private sealed class FakeReplayJobRepository : RepositoryFakeBase<ReplayJob>, IReplayJobRepository
    {
        public ReplayJob? LastAdded { get; private set; }

        public override Task AddAsync(ReplayJob job, CancellationToken cancellationToken = default)
        {
            LastAdded = job;
            return Task.CompletedTask;
        }

        public Task<ReplayJob?> GetByIdAsync(Ulid replayJobId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ReplayJob?>(null);

        public Task<ReplayJob?> GetByIdForUpdateAsync(Ulid replayJobId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ReplayJob?>(null);
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
