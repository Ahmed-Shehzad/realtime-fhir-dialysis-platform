using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using WorkflowOrchestrator.Application.Commands.StartWorkflowInstance;
using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace WorkflowOrchestrator.UnitTests;

public sealed class StartWorkflowInstanceCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_persists_workflow_and_records_auditAsync()
    {
        var repo = new FakeWorkflowInstanceRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-wf" };
        var handler = new StartWorkflowInstanceCommandHandler(repo, uow, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        var cmd = new StartWorkflowInstanceCommand(correlationId, "SessionCompletion", "sess-wf-1", "user-a");

        Ulid workflowId = await handler.HandleAsync(cmd);

        WorkflowInstance added = repo.LastAdded.ShouldNotBeNull();
        workflowId.ShouldBe(added.Id);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        audit.Records[0].ResourceType.ShouldBe("WorkflowInstance");
        audit.Records[0].TenantId.ShouldBe("tenant-wf");
        added.IntegrationEvents.Count.ShouldBe(1);
    }

    private sealed class FakeWorkflowInstanceRepository : RepositoryFakeBase<WorkflowInstance>, IWorkflowInstanceRepository
    {
        public WorkflowInstance? LastAdded { get; private set; }

        public override Task AddAsync(WorkflowInstance instance, CancellationToken cancellationToken = default)
        {
            LastAdded = instance;
            return Task.CompletedTask;
        }

        public Task<WorkflowInstance?> GetByIdAsync(Ulid workflowInstanceId, CancellationToken cancellationToken = default) =>
            Task.FromResult<WorkflowInstance?>(null);

        public Task<WorkflowInstance?> GetByIdForUpdateAsync(Ulid workflowInstanceId, CancellationToken cancellationToken = default) =>
            Task.FromResult<WorkflowInstance?>(null);
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
