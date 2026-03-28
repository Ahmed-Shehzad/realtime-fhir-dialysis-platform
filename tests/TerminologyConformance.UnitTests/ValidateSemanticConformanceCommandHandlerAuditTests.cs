using TerminologyConformance.Application.Commands.ValidateSemanticConformance;
using TerminologyConformance.Domain;
using TerminologyConformance.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace TerminologyConformance.UnitTests;

public sealed class ValidateSemanticConformanceCommandHandlerAuditTests
{
    [Fact]
    public async Task Validate_records_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakeConformanceRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-tc" };
        var handler = new ValidateSemanticConformanceCommandHandler(repo, uow, audit, tenant);
        var cmd = new ValidateSemanticConformanceCommand(
            correlationId,
            "r-1",
            "http://loinc.org",
            "8480-6",
            null,
            null,
            "user-z");

        ValidateSemanticConformanceResult result = await handler.HandleAsync(cmd);

        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Execute);
        r.ResourceType.ShouldBe("ConformanceAssessment");
        r.ResourceId.ShouldBe(result.AssessmentId.ToString());
        r.UserId.ShouldBe("user-z");
        ConformanceAssessment added = repo.LastAdded.ShouldNotBeNull();
        added.ResourceId.ShouldBe("r-1");
    }

    private sealed class FakeConformanceRepository : RepositoryFakeBase<ConformanceAssessment>, IConformanceAssessmentRepository
    {
        public ConformanceAssessment? LastAdded { get; private set; }

        public override Task AddAsync(ConformanceAssessment assessment, CancellationToken cancellationToken = default)
        {
            LastAdded = assessment;
            return Task.CompletedTask;
        }

        public Task<ConformanceAssessment?> GetLatestByResourceIdAsync(string resourceId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ConformanceAssessment?>(null);
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
