using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.RequestWorkflowManualIntervention;

public sealed record RequestWorkflowManualInterventionCommand(
    Ulid CorrelationId,
    Ulid WorkflowInstanceId,
    string Detail,
    string? AuthenticatedUserId = null) : ICommand;
