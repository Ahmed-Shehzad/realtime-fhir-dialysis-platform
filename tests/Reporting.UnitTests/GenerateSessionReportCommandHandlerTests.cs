using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Reporting.Application.Commands.GenerateSessionReport;
using Reporting.Domain;
using Reporting.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace Reporting.UnitTests;

public sealed class GenerateSessionReportCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_persists_report_and_records_auditAsync()
    {
        var repo = new FakeSessionReportRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-rpt" };
        var handler = new GenerateSessionReportCommandHandler(repo, uow, audit, tenant);
        Ulid correlationId = Ulid.NewUlid();
        var cmd = new GenerateSessionReportCommand(correlationId, "sess-rpt-1", "narrative-v1", null, "user-a");

        Ulid reportId = await handler.HandleAsync(cmd);

        SessionReport added = repo.LastAdded.ShouldNotBeNull();
        reportId.ShouldBe(added.Id);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        audit.Records[0].ResourceType.ShouldBe("SessionReport");
        audit.Records[0].TenantId.ShouldBe("tenant-rpt");
        added.IntegrationEvents.Count.ShouldBe(1);
    }

    private sealed class FakeSessionReportRepository : RepositoryFakeBase<SessionReport>, ISessionReportRepository
    {
        public SessionReport? LastAdded { get; private set; }

        public override Task AddAsync(SessionReport report, CancellationToken cancellationToken = default)
        {
            LastAdded = report;
            return Task.CompletedTask;
        }

        public Task<SessionReport?> GetByIdAsync(Ulid reportId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SessionReport?>(null);

        public Task<SessionReport?> GetByIdForUpdateAsync(Ulid reportId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SessionReport?>(null);
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
