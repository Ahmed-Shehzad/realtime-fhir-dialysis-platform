using ClinicalInteroperability.Application.Commands.RetryCanonicalObservationPublication;
using ClinicalInteroperability.Domain;
using ClinicalInteroperability.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace ClinicalInteroperability.UnitTests;

public sealed class RetryCanonicalObservationPublicationCommandHandlerAuditTests
{
    [Fact]
    public async Task Retry_records_audit_when_publication_foundAsync()
    {
        var correlationId = Ulid.NewUlid();
        var pub = CanonicalObservationPublication.StartPublication(
            Ulid.NewUlid(),
            "m-transient-once",
            null,
            null);
        var repo = new StubPublicationRepository(pub);
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new RetryCanonicalObservationPublicationCommandHandler(repo, uow, audit, tenant);
        var cmd = new RetryCanonicalObservationPublicationCommand(correlationId, pub.Id, "user-r");

        RetryCanonicalObservationPublicationResult result = await handler.HandleAsync(cmd);

        result.Found.ShouldBeTrue();
        result.State.ShouldBe(CanonicalPublicationState.Published);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        audit.Records[0].UserId.ShouldBe("user-r");
    }

    private sealed class StubPublicationRepository : RepositoryFakeBase<CanonicalObservationPublication>, ICanonicalObservationPublicationRepository
    {
        private readonly CanonicalObservationPublication _publication;

        public StubPublicationRepository(CanonicalObservationPublication publication) =>
            _publication = publication;

        public Task<CanonicalObservationPublication?> GetLatestByMeasurementIdAsync(
            string measurementId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<CanonicalObservationPublication?>(null);

        public Task<CanonicalObservationPublication?> GetByIdForUpdateAsync(
            Ulid publicationId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(publicationId == _publication.Id ? _publication : null);
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
