namespace WorkflowOrchestrator.Application.Queries.GetWorkflowInstanceById;

public sealed record WorkflowInstanceReadDto(
    string Id,
    string WorkflowKind,
    string State,
    string TreatmentSessionId,
    int StepOrdinal,
    string CurrentStepName,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
