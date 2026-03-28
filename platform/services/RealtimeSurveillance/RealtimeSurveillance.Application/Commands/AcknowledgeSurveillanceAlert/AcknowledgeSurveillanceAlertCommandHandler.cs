using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;

namespace RealtimeSurveillance.Application.Commands.AcknowledgeSurveillanceAlert;

public sealed class AcknowledgeSurveillanceAlertCommandHandler : ICommandHandler<AcknowledgeSurveillanceAlertCommand>
{
    private readonly ISurveillanceAlertRepository _alerts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public AcknowledgeSurveillanceAlertCommandHandler(
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
        AcknowledgeSurveillanceAlertCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        SurveillanceAlert? alert = await _alerts
            .GetByIdForUpdateAsync(command.AlertId, cancellationToken)
            .ConfigureAwait(false)
            ?? throw new KeyNotFoundException($"Alert {command.AlertId} was not found.");

        alert.Acknowledge(command.CorrelationId, command.AcknowledgedByUserId, _tenant.TenantId);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Update,
                    "SurveillanceAlert",
                    alert.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    "Alert acknowledged.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
    }
}
