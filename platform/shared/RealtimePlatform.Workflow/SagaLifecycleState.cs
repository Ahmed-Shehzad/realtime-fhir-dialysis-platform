namespace RealtimePlatform.Workflow;

/// <summary>
/// High-level saga states (aligns with realtime platform plan — explicit, persisted transitions).
/// </summary>
public enum SagaLifecycleState
{
    Started = 0,
    InProgress = 1,
    WaitingForEvent = 2,
    WaitingForTimeout = 3,
    Compensating = 4,
    Completed = 5,
    Failed = 6,
    ManualInterventionRequired = 7
}
