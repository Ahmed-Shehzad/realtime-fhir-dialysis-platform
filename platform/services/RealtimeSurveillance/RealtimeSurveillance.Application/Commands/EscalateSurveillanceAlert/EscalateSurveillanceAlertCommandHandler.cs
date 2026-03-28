using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

namespace RealtimeSurveillance.Application.Commands.EscalateSurveillanceAlert;

public sealed class EscalateSurveillanceAlertCommandHandler : ICommandHandler<EscalateSurveillanceAlertCommand>
{
    private readonly ISurveillanceAlertRepository _alerts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public EscalateSurveillanceAlertCommandHandler(
        ISurveillanceAlertRepository alerts,
        IUnitOfWork unitOfWork,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _alerts = alerts ?? throw new ArgumentNullException(nameof(alerts));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(
        EscalateSurveillanceAlertCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        SurveillanceAlert? alert = await _alerts
            .GetByIdForUpdateAsync(command.AlertId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Alert {command.AlertId} was not found.");

        alert.Escalate(command.CorrelationId, command.EscalationDetail, _tenant.TenantId);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "SurveillanceAlert",
                    alert.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Alert escalated.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
