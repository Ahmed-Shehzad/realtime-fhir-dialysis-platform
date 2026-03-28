using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using DeviceRegistry.Application.Commands.RegisterDevice;
using DeviceRegistry.Domain;
using DeviceRegistry.Domain.Abstractions;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace DeviceRegistry.UnitTests;

public sealed class RegisterDeviceCommandHandlerAuditTests
{
    [Fact]
    public async Task Successful_register_records_audit_before_saveAsync()
    {
        var correlationId = Ulid.NewUlid();
        const string deviceIdentifier = "device-audit-1";
        var repo = new FakeDeviceRepository { Existing = null };
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext { TenantId = "tenant-dr" };
        var handler = new RegisterDeviceCommandHandler(repo, uow, audit, tenant);
        var cmd = new RegisterDeviceCommand(
            correlationId,
            deviceIdentifier,
            "Acme",
            "Active",
            AuthenticatedUserId: "user-oid-99");

        RegisterDeviceResult result = await handler.HandleAsync(cmd);

        result.DeviceId.ShouldBe(deviceIdentifier);
        uow.SaveChangesCallCount.ShouldBe(1);
        audit.Records.Count.ShouldBe(1);
        AuditRecordRequest r = audit.Records[0];
        r.Action.ShouldBe(AuditAction.Create);
        r.ResourceType.ShouldBe("Device");
        r.ResourceId.ShouldBe(deviceIdentifier);
        r.UserId.ShouldBe("user-oid-99");
        r.Outcome.ShouldBe(AuditOutcome.Success);
        r.TenantId.ShouldBe("tenant-dr");
        r.CorrelationId.ShouldBe(correlationId.ToString());
    }

    [Fact]
    public async Task Duplicate_device_throws_and_does_not_record_auditAsync()
    {
        var deviceId = new DeviceId("dup-dev");
        Device existing = Device.Register(Ulid.NewUlid(), deviceId, TrustState.Active, null, "t1");
        var repo = new FakeDeviceRepository { Existing = existing };
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new RegisterDeviceCommandHandler(repo, uow, audit, tenant);
        var cmd = new RegisterDeviceCommand(Ulid.NewUlid(), deviceId.Value, null, "Active");

        _ = await Should.ThrowAsync<InvalidOperationException>(() => handler.HandleAsync(cmd));

        audit.Records.ShouldBeEmpty();
        uow.SaveChangesCallCount.ShouldBe(0);
    }

    private sealed class FakeDeviceRepository : RepositoryFakeBase<Device>, IDeviceRepository
    {
        public Device? Existing { get; init; }

        public List<Device> Added { get; } = [];

        public Task<Device?> GetByDeviceIdAsync(DeviceId deviceId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Existing);

        public override Task AddAsync(Device device, CancellationToken cancellationToken = default)
        {
            Added.Add(device);
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
        public string TenantId { get; init; } = "default";
    }
}
