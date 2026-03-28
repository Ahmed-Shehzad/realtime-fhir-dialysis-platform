using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Queries.GetWorkflowInstanceById;

public sealed record GetWorkflowInstanceByIdQuery(Ulid WorkflowInstanceId) : IQuery<WorkflowInstanceReadDto?>;
