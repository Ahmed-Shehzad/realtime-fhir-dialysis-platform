using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

using RealtimeDelivery.Application.Abstractions;
using RealtimeDelivery.Domain.Contracts;

namespace RealtimeDelivery.Application.Commands.BroadcastAlertFeed;

public sealed class BroadcastAlertFeedCommandHandler : ICommandHandler<BroadcastAlertFeedCommand>
{
    private readonly IRealtimeFeedGateway _gateway;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public BroadcastAlertFeedCommandHandler(
        IRealtimeFeedGateway gateway,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(BroadcastAlertFeedCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.EventType))
            throw new ArgumentException("EventType is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.AlertId))
            throw new ArgumentException("AlertId is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.Severity))
            throw new ArgumentException("Severity is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.LifecycleState))
            throw new ArgumentException("LifecycleState is required.", nameof(command));

        string eventType = command.EventType.Trim();
        string? sessionPart = string.IsNullOrWhiteSpace(command.TreatmentSessionId)
            ? null
            : command.TreatmentSessionId.Trim();
        string alertId = command.AlertId.Trim();
        string severity = command.Severity.Trim();
        string lifecycle = command.LifecycleState.Trim();

        var payload = new AlertFeedPayload(
            eventType,
            sessionPart,
            alertId,
            severity,
            lifecycle,
            command.OccurredAtUtc);

        await _gateway.PushAlertAsync(_tenant.TenantId, payload, cancellationToken).ConfigureAwait(false);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "ClinicalFeedAlert",
                    alertId,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Alert feed broadcast: eventType={eventType}, severity={severity}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
