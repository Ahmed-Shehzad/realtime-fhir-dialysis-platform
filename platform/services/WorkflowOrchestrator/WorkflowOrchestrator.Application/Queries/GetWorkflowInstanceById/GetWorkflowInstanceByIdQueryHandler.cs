using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.Abstractions;

using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Queries.GetWorkflowInstanceById;

public sealed class GetWorkflowInstanceByIdQueryHandler : IQueryHandler<GetWorkflowInstanceByIdQuery, WorkflowInstanceReadDto?>
{
    private readonly IWorkflowInstanceRepository _workflows;

    public GetWorkflowInstanceByIdQueryHandler(IWorkflowInstanceRepository workflows) =>
        _workflows = workflows ?? throw new ArgumentNullException(nameof(workflows));

    public async Task<WorkflowInstanceReadDto?> HandleAsync(
        GetWorkflowInstanceByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        WorkflowInstance? row = await _workflows
            .GetByIdAsync(query.WorkflowInstanceId, cancellationToken)
            .ConfigureAwait(false);
        if (row is null)
            return null;

        return new WorkflowInstanceReadDto(
            row.Id.ToString(),
            row.Kind.Value,
            row.State.Value,
            row.TreatmentSessionId.Value,
            row.StepOrdinal,
            row.CurrentStepName.Value,
            row.CreatedAtUtc,
            row.UpdatedAtUtc);
    }
}
