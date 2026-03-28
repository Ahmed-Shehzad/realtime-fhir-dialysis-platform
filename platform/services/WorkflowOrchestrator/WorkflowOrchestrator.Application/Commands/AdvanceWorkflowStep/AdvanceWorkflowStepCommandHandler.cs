using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.Abstractions;
using WorkflowOrchestrator.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.AdvanceWorkflowStep;

public sealed class AdvanceWorkflowStepCommandHandler : ICommandHandler<AdvanceWorkflowStepCommand>
{
    private readonly IWorkflowInstanceRepository _workflows;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public AdvanceWorkflowStepCommandHandler(
        IWorkflowInstanceRepository workflows,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _workflows = workflows ?? throw new ArgumentNullException(nameof(workflows));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        AdvanceWorkflowStepCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.NextStepName))
            throw new ArgumentException("NextStepName is required.", nameof(command));

        WorkflowInstance? instance = await _workflows
            .GetByIdForUpdateAsync(command.WorkflowInstanceId, cancellationToken)
            .ConfigureAwait(false);
        if (instance is null)
            throw new InvalidOperationException($"Workflow instance {command.WorkflowInstanceId} was not found.");

        var nextName = new WorkflowStepName(command.NextStepName.Trim());
        instance.AdvanceStep(nextName);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "WorkflowInstance",
                    instance.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Workflow advanced to step {nextName.Value} (ordinal {instance.StepOrdinal}).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
