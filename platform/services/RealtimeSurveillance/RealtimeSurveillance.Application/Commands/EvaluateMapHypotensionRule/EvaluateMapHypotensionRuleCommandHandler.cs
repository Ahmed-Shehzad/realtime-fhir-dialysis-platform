using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;
using BuildingBlocks.ValueObjects;

using Intercessor.Abstractions;

using RealtimeSurveillance.Domain;
using RealtimeSurveillance.Domain.Abstractions;
using RealtimeSurveillance.Domain.ValueObjects;

namespace RealtimeSurveillance.Application.Commands.EvaluateMapHypotensionRule;

public sealed class EvaluateMapHypotensionRuleCommandHandler
    : ICommandHandler<EvaluateMapHypotensionRuleCommand, EvaluateMapHypotensionRuleResult>
{
    private const string MapBelow65RuleCode = "MAP_BELOW_65";
    private const double MapHypotensionThresholdMmHg = 65.0;

    private readonly ISurveillanceAlertRepository _alerts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public EvaluateMapHypotensionRuleCommandHandler(
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

    public async Task<EvaluateMapHypotensionRuleResult> HandleAsync(
        EvaluateMapHypotensionRuleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (!string.Equals(command.RuleCode.Trim(), MapBelow65RuleCode, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Unsupported rule code: {command.RuleCode}.", nameof(command));

        if (command.MetricValueMmHg >= MapHypotensionThresholdMmHg)
            return new EvaluateMapHypotensionRuleResult(false, null);

        var sessionId = new SessionId(command.TreatmentSessionId);
        var type = new AlertTypeCode("HYPOTENSION_MAP");
        var severity = new AlertSeverityLevel("High");
        string detail = $"MAP {command.MetricValueMmHg:F1} mmHg below threshold {MapHypotensionThresholdMmHg}.";

        SurveillanceAlert alert = SurveillanceAlert.Raise(
            command.CorrelationId,
            sessionId,
            type,
            severity,
            detail,
            _tenant.TenantId);

        await _alerts.AddAsync(alert, cancellationToken).ConfigureAwait(false);
        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "SurveillanceRule",
                    MapBelow65RuleCode,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Rule fired; alert {alert.Id} raised.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);

        _ = await _unitOfWork.CommitAsync(cancellationToken).ConfigureAwait(false);
        return new EvaluateMapHypotensionRuleResult(true, alert.Id);
    }
}
