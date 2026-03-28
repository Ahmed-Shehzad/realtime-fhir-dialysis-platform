using BuildingBlocks.Abstractions;

namespace WorkflowOrchestrator.Domain.Abstractions;

public interface IWorkflowInstanceRepository : IRepository<WorkflowOrchestrator.Domain.WorkflowInstance>
{
    Task<WorkflowOrchestrator.Domain.WorkflowInstance?> GetByIdAsync(
        Ulid workflowInstanceId,
        CancellationToken cancellationToken = default);

    Task<WorkflowOrchestrator.Domain.WorkflowInstance?> GetByIdForUpdateAsync(
        Ulid workflowInstanceId,
        CancellationToken cancellationToken = default);
}
