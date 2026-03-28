using BuildingBlocks;
using BuildingBlocks.ValueObjects;

using RealtimePlatform.IntegrationEventCatalog;

using WorkflowOrchestrator.Domain.ValueObjects;

namespace WorkflowOrchestrator.Domain;

public sealed class WorkflowInstance : AggregateRoot
{
    private WorkflowInstance()
    {
    }

    public WorkflowKind Kind { get; private set; } = null!;

    public WorkflowLifecycleState State { get; private set; } = null!;

    public SessionId TreatmentSessionId { get; private set; }

    public int StepOrdinal { get; private set; }

    public WorkflowStepName CurrentStepName { get; private set; } = null!;

    public static WorkflowInstance Start(
        Ulid correlationId,
        WorkflowKind kind,
        SessionId treatmentSessionId,
        string? tenantId)
    {
        ArgumentNullException.ThrowIfNull(kind);
        var instance = new WorkflowInstance
        {
            Kind = kind,
            State = WorkflowLifecycleState.Running,
            TreatmentSessionId = treatmentSessionId,
            StepOrdinal = 0,
            CurrentStepName = new WorkflowStepName("Started"),
        };
        instance.ApplyCreatedDateTime();
        string sessionStr = treatmentSessionId.Value;
        string idStr = instance.Id.ToString();
        instance.ApplyEvent(
            new WorkflowStartedIntegrationEvent(correlationId, idStr, kind.Value, sessionStr)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
        return instance;
    }

    public void AdvanceStep(WorkflowStepName nextStepName)
    {
        ArgumentNullException.ThrowIfNull(nextStepName);
        if (State != WorkflowLifecycleState.Running)
            throw new InvalidOperationException("Only running workflows can advance.");

        StepOrdinal = checked(StepOrdinal + 1);
        CurrentStepName = nextStepName;
        ApplyUpdateDateTime();
    }

    public void Complete(Ulid correlationId, string? tenantId)
    {
        if (State != WorkflowLifecycleState.Running)
            throw new InvalidOperationException("Only running workflows can complete.");

        State = WorkflowLifecycleState.Completed;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new WorkflowCompletedIntegrationEvent(correlationId, Id.ToString(), Kind.Value, StepOrdinal)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }

    public void Fail(Ulid correlationId, string reason, string? tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (State != WorkflowLifecycleState.Running
            && State != WorkflowLifecycleState.Compensating
            && State != WorkflowLifecycleState.ManualInterventionRequired)
            throw new InvalidOperationException("Workflow cannot fail from the current state.");

        State = WorkflowLifecycleState.Failed;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new WorkflowFailedIntegrationEvent(correlationId, Id.ToString(), reason.Trim())
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }

    public void TriggerCompensation(Ulid correlationId, string reason, string? tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        if (State != WorkflowLifecycleState.Running)
            throw new InvalidOperationException("Only running workflows can trigger compensation.");

        State = WorkflowLifecycleState.Compensating;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new CompensationTriggeredIntegrationEvent(correlationId, Id.ToString(), reason.Trim())
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }

    public void RequestManualIntervention(Ulid correlationId, string detail, string? tenantId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(detail);
        if (State != WorkflowLifecycleState.Running && State != WorkflowLifecycleState.Compensating)
            throw new InvalidOperationException("Manual intervention can be requested only from Running or Compensating.");

        State = WorkflowLifecycleState.ManualInterventionRequired;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new ManualInterventionRequestedIntegrationEvent(correlationId, Id.ToString(), detail.Trim())
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }

    public void SignalTimeout(Ulid correlationId, string? tenantId)
    {
        if (State != WorkflowLifecycleState.Running)
            throw new InvalidOperationException("Only running workflows can record timeout.");

        State = WorkflowLifecycleState.TimedOut;
        ApplyUpdateDateTime();
        string sessionStr = TreatmentSessionId.Value;
        ApplyEvent(
            new TimeoutElapsedIntegrationEvent(
                correlationId,
                Id.ToString(),
                CurrentStepName.Value,
                StepOrdinal)
            {
                SessionId = sessionStr,
                TenantId = tenantId,
            });
    }
}
