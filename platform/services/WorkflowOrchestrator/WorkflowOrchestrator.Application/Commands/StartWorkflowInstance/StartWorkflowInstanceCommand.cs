using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.StartWorkflowInstance;

public sealed record StartWorkflowInstanceCommand(
    Ulid CorrelationId,
    string WorkflowKind,
    string TreatmentSessionId,
    string? AuthenticatedUserId = null) : ICommand<Ulid>;
