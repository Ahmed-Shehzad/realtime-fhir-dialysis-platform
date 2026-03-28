using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.FailWorkflowInstance;

public sealed record FailWorkflowInstanceCommand(
    Ulid CorrelationId,
    Ulid WorkflowInstanceId,
    string Reason,
    string? AuthenticatedUserId = null) : ICommand;
