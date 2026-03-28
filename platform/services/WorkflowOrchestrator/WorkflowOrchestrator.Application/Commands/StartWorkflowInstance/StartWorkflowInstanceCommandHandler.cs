using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using WorkflowOrchestrator.Domain;
using WorkflowOrchestrator.Domain.Abstractions;
using WorkflowOrchestrator.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace WorkflowOrchestrator.Application.Commands.StartWorkflowInstance;

public sealed class StartWorkflowInstanceCommandHandler : ICommandHandler<StartWorkflowInstanceCommand, Ulid>
{
    private readonly IWorkflowInstanceRepository _workflows;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public StartWorkflowInstanceCommandHandler(
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

    public async Task<Ulid> HandleAsync(
        StartWorkflowInstanceCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.TreatmentSessionId))
            throw new ArgumentException("TreatmentSessionId is required.", nameof(command));

        var kind = WorkflowKind.ParseApi(command.WorkflowKind);
        var sessionId = new SessionId(command.TreatmentSessionId.Trim());
        WorkflowInstance instance = WorkflowInstance.Start(
            command.CorrelationId,
            kind,
            sessionId,
            _tenant.TenantId);

        await _workflows.AddAsync(instance, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "WorkflowInstance",
                    instance.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Workflow {kind.Value} started for session {sessionId.Value}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return instance.Id;
    }
}
