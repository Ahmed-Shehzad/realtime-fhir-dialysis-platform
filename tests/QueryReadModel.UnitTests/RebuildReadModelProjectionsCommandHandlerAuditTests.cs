using QueryReadModel.Application.Abstractions;
using QueryReadModel.Application.Commands.RebuildReadModelProjections;
using QueryReadModel.Domain;
using QueryReadModel.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace QueryReadModel.UnitTests;

public sealed class RebuildReadModelProjectionsCommandHandlerAuditTests
{
    [Fact]
    public async Task Rebuild_runs_maintenance_records_audit_and_persists_rebuild_recordAsync()
    {
        Ulid correlationId = Ulid.NewUlid();
        var maintenance = new StubMaintenance { ReturnCount = 3 };
        var rebuildRepo = new CapturingRebuildRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new RebuildReadModelProjectionsCommandHandler(maintenance, rebuildRepo, uow, audit, tenant);
        var cmd = new RebuildReadModelProjectionsCommand(correlationId, "user-r");

        int count = await handler.HandleAsync(cmd);

        count.ShouldBe(3);
        maintenance.Called.ShouldBeTrue();
        rebuildRepo.Added.Count.ShouldBe(1);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        audit.Records[0].UserId.ShouldBe("user-r");
        audit.Records[0].Action.ShouldBe(AuditAction.Execute);
    }

    private sealed class StubMaintenance : IReadModelProjectionMaintenance
    {
        public bool Called { get; private set; }

        public int ReturnCount { get; init; } = 2;

        public Task<int> ClearAndSeedStubAsync(CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(ReturnCount);
        }
    }

    private sealed class CapturingRebuildRepository : RepositoryFakeBase<ReadModelRebuildRecord>, IReadModelRebuildRecordRepository
    {
        public List<ReadModelRebuildRecord> Added { get; } = [];

        public override Task AddAsync(ReadModelRebuildRecord row, CancellationToken cancellationToken = default)
        {
            Added.Add(row);
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
        public string TenantId { get; init; } = "t1";
    }
}
