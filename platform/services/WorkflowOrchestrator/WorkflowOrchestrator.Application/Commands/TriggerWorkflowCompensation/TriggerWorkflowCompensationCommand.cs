using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.TriggerWorkflowCompensation;

public sealed record TriggerWorkflowCompensationCommand(
    Ulid CorrelationId,
    Ulid WorkflowInstanceId,
    string Reason,
    string? AuthenticatedUserId = null) : ICommand;
