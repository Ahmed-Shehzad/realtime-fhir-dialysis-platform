using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.Abstractions;

using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.SignalWorkflowTimeout;

public sealed class SignalWorkflowTimeoutCommandHandler : ICommandHandler<SignalWorkflowTimeoutCommand>
{
    private readonly IWorkflowInstanceRepository _workflows;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public SignalWorkflowTimeoutCommandHandler(
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
        SignalWorkflowTimeoutCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        WorkflowInstance? instance = await _workflows
            .GetByIdForUpdateAsync(command.WorkflowInstanceId, cancellationToken)
            .ConfigureAwait(false);
        if (instance is null)
            throw new InvalidOperationException($"Workflow instance {command.WorkflowInstanceId} was not found.");

        instance.SignalTimeout(command.CorrelationId, _tenant.TenantId);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "WorkflowInstance",
                    instance.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Workflow timeout recorded.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
