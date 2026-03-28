using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using TreatmentSession.Application.Commands.CreateSession;
using TreatmentSession.Domain;
using TreatmentSession.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace TreatmentSession.UnitTests;

public sealed class CreateDialysisSessionCommandHandlerAuditTests
{
    [Fact]
    public async Task Create_records_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakeSessionRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-ts" };
        var handler = new CreateDialysisSessionCommandHandler(repo, uow, audit, tenant);
        var cmd = new CreateDialysisSessionCommand(correlationId, "user-x");

        CreateDialysisSessionResult result = await handler.HandleAsync(cmd);

        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Create);
        r.ResourceType.ShouldBe("DialysisSession");
        r.ResourceId.ShouldBe(result.SessionId.ToString());
        r.UserId.ShouldBe("user-x");
        r.TenantId.ShouldBe("tenant-ts");
        _ = repo.LastAdded.ShouldBeOfType<DialysisSession>();
    }

    private sealed class FakeSessionRepository : RepositoryFakeBase<DialysisSession>, ISessionRepository
    {
        public DialysisSession? LastAdded { get; private set; }

        public override Task AddAsync(DialysisSession session, CancellationToken cancellationToken = default)
        {
            LastAdded = session;
            return Task.CompletedTask;
        }

        public Task<DialysisSession?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<DialysisSession?>(null);
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
