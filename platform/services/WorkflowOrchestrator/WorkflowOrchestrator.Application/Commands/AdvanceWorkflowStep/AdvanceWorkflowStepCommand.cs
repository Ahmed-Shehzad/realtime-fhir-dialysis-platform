using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.AdvanceWorkflowStep;

public sealed record AdvanceWorkflowStepCommand(
    Ulid CorrelationId,
    Ulid WorkflowInstanceId,
    string NextStepName,
    string? AuthenticatedUserId = null) : ICommand;
