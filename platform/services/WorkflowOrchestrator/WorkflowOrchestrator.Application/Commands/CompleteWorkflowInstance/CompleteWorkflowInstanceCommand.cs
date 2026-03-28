using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.CompleteWorkflowInstance;

public sealed record CompleteWorkflowInstanceCommand(
    Ulid CorrelationId,
    Ulid WorkflowInstanceId,
    string? AuthenticatedUserId = null) : ICommand;
