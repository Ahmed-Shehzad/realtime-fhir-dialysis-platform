using AuditProvenance.Application.Commands.RecordPlatformAuditFact;

using AuditProvenance.Domain;
using AuditProvenance.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace AuditProvenance.UnitTests;

public sealed class RecordPlatformAuditFactCommandHandlerAuditTests
{
    [Fact]
    public async Task Record_records_security_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakeFactRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-ap" };
        var handler = new RecordPlatformAuditFactCommandHandler(repo, uow, audit, tenant);
        var cmd = new RecordPlatformAuditFactCommand(
            correlationId,
            DateTimeOffset.UtcNow,
            "Test.Event",
            "Summary",
            null,
            null,
            null,
            null,
            "AuditProvenance.Api",
            null,
            null,
            null,
            null,
            "user-y");

        RecordPlatformAuditFactResult result = await handler.HandleAsync(cmd);

        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Create);
        r.ResourceType.ShouldBe("PlatformAuditFact");
        r.ResourceId.ShouldBe(result.PlatformAuditFactId.ToString());
        r.UserId.ShouldBe("user-y");
        r.TenantId.ShouldBe("tenant-ap");
        PlatformAuditFact added = repo.LastAdded.ShouldNotBeNull();
        added.EventType.ShouldBe("Test.Event");
    }

    private sealed class FakeFactRepository : RepositoryFakeBase<PlatformAuditFact>, IPlatformAuditFactRepository
    {
        public PlatformAuditFact? LastAdded { get; private set; }

        public override Task AddAsync(PlatformAuditFact fact, CancellationToken cancellationToken = default)
        {
            LastAdded = fact;
            return Task.CompletedTask;
        }

        public Task<PlatformAuditFact?> GetByIdAsync(Ulid id, CancellationToken cancellationToken = default) =>
            Task.FromResult<PlatformAuditFact?>(null);

        public Task<IReadOnlyList<PlatformAuditFactSummary>> GetRecentSummariesAsync(int count, CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<PlatformAuditFactSummary>>(Array.Empty<PlatformAuditFactSummary>());
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
