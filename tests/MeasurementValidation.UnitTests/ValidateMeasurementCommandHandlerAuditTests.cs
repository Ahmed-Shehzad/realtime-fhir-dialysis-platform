using MeasurementValidation.Application.Commands.ValidateMeasurement;
using MeasurementValidation.Domain;
using MeasurementValidation.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace MeasurementValidation.UnitTests;

public sealed class ValidateMeasurementCommandHandlerAuditTests
{
    [Fact]
    public async Task Validate_records_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        var repo = new FakeValidationRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-mv" };
        var handler = new ValidateMeasurementCommandHandler(repo, uow, audit, tenant);
        var cmd = new ValidateMeasurementCommand(correlationId, "m-x", "prof-1", 10d, "user-z");

        ValidateMeasurementResult result = await handler.HandleAsync(cmd);

        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Execute);
        r.ResourceType.ShouldBe("ValidatedMeasurement");
        r.ResourceId.ShouldBe(result.ValidationId.ToString());
        r.UserId.ShouldBe("user-z");
        r.TenantId.ShouldBe("tenant-mv");
        ValidatedMeasurement added = repo.LastAdded.ShouldNotBeNull();
        added.MeasurementId.ShouldBe("m-x");
    }

    private sealed class FakeValidationRepository : RepositoryFakeBase<ValidatedMeasurement>, IMeasurementValidationRepository
    {
        public ValidatedMeasurement? LastAdded { get; private set; }

        public override Task AddAsync(ValidatedMeasurement measurementValidation, CancellationToken cancellationToken = default)
        {
            LastAdded = measurementValidation;
            return Task.CompletedTask;
        }

        public Task<ValidatedMeasurement?> GetLatestByMeasurementIdAsync(string measurementId, CancellationToken cancellationToken = default) =>
            Task.FromResult<ValidatedMeasurement?>(null);
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
