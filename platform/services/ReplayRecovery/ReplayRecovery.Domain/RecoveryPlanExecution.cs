using BuildingBlocks;

using RealtimePlatform.IntegrationEventCatalog;

using ReplayRecovery.Domain.ValueObjects;

namespace ReplayRecovery.Domain;

public sealed class RecoveryPlanExecution : AggregateRoot
{
    private RecoveryPlanExecution()
    {
    }

    public RecoveryPlanCode PlanCode { get; private set; } = null!;

    public RecoveryExecutionState State { get; private set; } = null!;

    public string OutcomeSummary { get; private set; } = null!;

    /// <summary>MVP synthetic execution: persisted outcome marker for operator-triggered recovery.</summary>
    public static RecoveryPlanExecution RunMvp(
        Ulid correlationId,
        RecoveryPlanCode planCode,
        string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(planCode);
        const string outcome = "MVP synthetic success (coordination only; extend for real recovery steps).";
        var execution = new RecoveryPlanExecution
        {
            PlanCode = planCode,
            State = RecoveryExecutionState.Executed,
            OutcomeSummary = outcome,
        };
        execution.ApplyCreatedDateTime();
        execution.ApplyUpdateDateTime();
        execution.ApplyEvent(
            new RecoveryPlanExecutedIntegrationEvent(
                correlationId,
                execution.Id.ToString(),
                planCode.Value,
                outcome)
            {
                TenantId = tenantId,
            });
        return execution;
    }
}
