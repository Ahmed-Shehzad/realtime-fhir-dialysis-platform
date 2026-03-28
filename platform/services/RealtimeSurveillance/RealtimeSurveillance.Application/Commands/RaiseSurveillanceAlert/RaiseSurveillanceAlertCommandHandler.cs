using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using Intercessor.Abstractions;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;
using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Application.Commands.RaiseSurveillanceAlert;

public sealed class RaiseSurveillanceAlertCommandHandler : ICommandHandler<RaiseSurveillanceAlertCommand, Ulid>
{
    private readonly ISurveillanceAlertRepository _alerts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public RaiseSurveillanceAlertCommandHandler(
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

    public async Task<Ulid> HandleAsync(
        RaiseSurveillanceAlertCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        var sessionId = new SessionId(command.TreatmentSessionId);
        var type = new AlertTypeCode(command.AlertType);
        var severity = new AlertSeverityLevel(command.Severity);

        SurveillanceAlert alert = SurveillanceAlert.Raise(
            command.CorrelationId,
            sessionId,
            type,
            severity,
            command.Detail,
            _tenant.TenantId);

        await _alerts.AddAsync(alert, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Create,
                    "SurveillanceAlert",
                    alert.Id.ToString(),
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Alert raised: type={type.Value}, severity={severity.Value}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return alert.Id;
    }
}
