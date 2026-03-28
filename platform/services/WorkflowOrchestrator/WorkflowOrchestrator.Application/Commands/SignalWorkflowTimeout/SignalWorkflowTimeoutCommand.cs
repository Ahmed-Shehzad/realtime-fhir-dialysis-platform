using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.SignalWorkflowTimeout;

public sealed record SignalWorkflowTimeoutCommand(
    Ulid CorrelationId,
    Ulid WorkflowInstanceId,
    string? AuthenticatedUserId = null) : ICommand;
