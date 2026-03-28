using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using ClinicalAnalytics.Application.Commands.RunSessionAnalysis;
using ClinicalAnalytics.Domain;
using ClinicalAnalytics.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace ClinicalAnalytics.UnitTests;

public sealed class RunSessionAnalysisCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_persists_analysis_and_records_auditAsync()
    {
        var repo = new FakeSessionAnalysisRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-ca" };
        var handler = new RunSessionAnalysisCommandHandler(repo, uow, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        var cmd = new RunSessionAnalysisCommand(correlationId, "sess-1", "mvp-model-v1", "user-a");

        Ulid analysisId = await handler.HandleAsync(cmd);

        SessionAnalysis added = repo.LastAdded.ShouldNotBeNull();
        analysisId.ShouldBe(added.Id);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        audit.Records[0].ResourceType.ShouldBe("SessionAnalysis");
        audit.Records[0].TenantId.ShouldBe("tenant-ca");
        added.IntegrationEvents.Count.ShouldBe(5);
    }

    private sealed class FakeSessionAnalysisRepository : RepositoryFakeBase<SessionAnalysis>, ISessionAnalysisRepository
    {
        public SessionAnalysis? LastAdded { get; private set; }

        public override Task AddAsync(SessionAnalysis analysis, CancellationToken cancellationToken = default)
        {
            LastAdded = analysis;
            return Task.CompletedTask;
        }

        public Task<SessionAnalysis?> GetByIdAsync(Ulid analysisId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SessionAnalysis?>(null);
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
