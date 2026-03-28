using ClinicalInteroperability.Application.Commands.PublishCanonicalObservation;
using ClinicalInteroperability.Domain;
using ClinicalInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace ClinicalInteroperability.UnitTests;

public sealed class PublishCanonicalObservationCommandHandlerAuditTests
{
    [Fact]
    public async Task Publish_records_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakePublicationRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-ci" };
        var handler = new PublishCanonicalObservationCommandHandler(repo, uow, audit, tenant);
        var cmd = new PublishCanonicalObservationCommand(correlationId, "m-x", "http://profile", "user-1");

        PublishCanonicalObservationResult result = await handler.HandleAsync(cmd);

        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Execute);
        r.ResourceType.ShouldBe("CanonicalObservationPublication");
        r.ResourceId.ShouldBe(result.PublicationId.ToString());
        r.UserId.ShouldBe("user-1");
        CanonicalObservationPublication added = repo.LastAdded.ShouldNotBeNull();
        added.MeasurementId.ShouldBe("m-x");
    }

    private sealed class FakePublicationRepository : RepositoryFakeBase<CanonicalObservationPublication>, ICanonicalObservationPublicationRepository
    {
        public CanonicalObservationPublication? LastAdded { get; private set; }

        public override Task AddAsync(CanonicalObservationPublication publication, CancellationToken cancellationToken = default)
        {
            LastAdded = publication;
            return Task.CompletedTask;
        }

        public Task<CanonicalObservationPublication?> GetLatestByMeasurementIdAsync(
            string measurementId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CanonicalObservationPublication?>(null);

        public Task<CanonicalObservationPublication?> GetByIdForUpdateAsync(
            Ulid publicationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CanonicalObservationPublication?>(null);
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
