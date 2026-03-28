using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using ReplayRecovery.Domain;
using ReplayRecovery.Domain.Abstractions;
using ReplayRecovery.Domain.ValueObjects;

using Intercessor.Abstractions;

namespace ReplayRecovery.Application.Commands.ExecuteRecoveryPlan;

public sealed class ExecuteRecoveryPlanCommandHandler : ICommandHandler<ExecuteRecoveryPlanCommand, Ulid>
{
    private readonly IRecoveryPlanExecutionRepository _executions;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public ExecuteRecoveryPlanCommandHandler(
        IRecoveryPlanExecutionRepository executions,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _executions = executions ?? throw new ArgumentNullException(nameof(executions));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task<Ulid> HandleAsync(
        ExecuteRecoveryPlanCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.PlanCode))
            throw new ArgumentException("PlanCode is required.", nameof(command));

        var code = new RecoveryPlanCode(command.PlanCode);
        RecoveryPlanExecution execution = RecoveryPlanExecution.RunMvp(
            command.CorrelationId,
            code,
            _tenant.TenantId);

        await _executions.AddAsync(execution, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "RecoveryPlanExecution",
                    execution.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Recovery plan {code.Value} executed (MVP).",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return execution.Id;
    }
}
