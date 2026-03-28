using RealtimeSurveillance.Application.Commands.EvaluateMapHypotensionRule;
using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using RealtimePlatform.UnitTesting;

using Shouldly;

using Xunit;

namespace RealtimeSurveillance.UnitTests;

public sealed class EvaluateMapHypotensionRuleCommandHandlerTests
{
    [Fact]
    public async Task Below_threshold_raises_alertAsync()
    {
        var repo = new CapturingAlertRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new EvaluateMapHypotensionRuleCommandHandler(repo, uow, audit, tenant);
        var cmd = new EvaluateMapHypotensionRuleCommand(Ulid.NewUlid(), "sess-a", "MAP_BELOW_65", 62.0, "u1");

        EvaluateMapHypotensionRuleResult evalResult = await handler.HandleAsync(cmd);

        evalResult.AlertRaised.ShouldBeTrue();
        evalResult.AlertId.HasValue.ShouldBeTrue();
        repo.Added.Count.ShouldBe(1);
        uow.SaveChangesCallCount.ShouldBe(1);
    }

    [Fact]
    public async Task At_or_above_threshold_does_not_raiseAsync()
    {
        var repo = new CapturingAlertRepository();
        var uow = new FakeUnitOfWork();
        var audit = new CapturingAuditRecorder();
        var tenant = new StubTenantContext();
        var handler = new EvaluateMapHypotensionRuleCommandHandler(repo, uow, audit, tenant);
        var cmd = new EvaluateMapHypotensionRuleCommand(Ulid.NewUlid(), "sess-a", "MAP_BELOW_65", 70.0, "u1");

        EvaluateMapHypotensionRuleResult result = await handler.HandleAsync(cmd);

        result.AlertRaised.ShouldBeFalse();
        result.AlertId.ShouldBeNull();
        repo.Added.Count.ShouldBe(0);
        uow.SaveChangesCallCount.ShouldBe(0);
    }

    private sealed class CapturingAlertRepository : RepositoryFakeBase<SurveillanceAlert>, ISurveillanceAlertRepository
    {
        public List<SurveillanceAlert> Added { get; } = [];

        public override Task AddAsync(SurveillanceAlert alert, CancellationToken cancellationToken = default)
        {
            Added.Add(alert);
            return Task.CompletedTask;
        }

        public Task<SurveillanceAlert?> GetByIdForUpdateAsync(Ulid alertId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SurveillanceAlert?>(null);

        public Task<SurveillanceAlert?> GetByIdAsync(Ulid alertId, CancellationToken cancellationToken = default) =>
            Task.FromResult<SurveillanceAlert?>(null);

        public Task<IReadOnlyList<SurveillanceAlert>> ListBySessionAsync(
            string treatmentSessionId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<IReadOnlyList<SurveillanceAlert>>(Array.Empty<SurveillanceAlert>());
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
