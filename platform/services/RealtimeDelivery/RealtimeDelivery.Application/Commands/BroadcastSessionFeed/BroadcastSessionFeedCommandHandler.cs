using BuildingBlocks.Abstractions;
using BuildingBlocks.Tenancy;

using Intercessor.Abstractions;

using RealtimeDelivery.Application.Abstractions;
using RealtimeDelivery.Domain.Contracts;

namespace RealtimeDelivery.Application.Commands.BroadcastSessionFeed;

public sealed class BroadcastSessionFeedCommandHandler : ICommandHandler<BroadcastSessionFeedCommand>
{
    private readonly IRealtimeFeedGateway _gateway;
    private readonly IAuditRecorder _audit;
    private readonly ITenantContext _tenant;

    public BroadcastSessionFeedCommandHandler(
        IRealtimeFeedGateway gateway,
        IAuditRecorder audit,
        ITenantContext tenant)
    {
        _gateway = gateway ?? throw new ArgumentNullException(nameof(gateway));
        _audit = audit ?? throw new ArgumentNullException(nameof(audit));
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));
    }

    public async Task HandleAsync(BroadcastSessionFeedCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (string.IsNullOrWhiteSpace(command.TreatmentSessionId))
            throw new ArgumentException("TreatmentSessionId is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.EventType))
            throw new ArgumentException("EventType is required.", nameof(command));
        if (string.IsNullOrWhiteSpace(command.Summary))
            throw new ArgumentException("Summary is required.", nameof(command));

        string sessionId = command.TreatmentSessionId.Trim();
        string eventType = command.EventType.Trim();
        string summary = command.Summary.Trim();
        var payload = new SessionFeedPayload(eventType, sessionId, summary, command.OccurredAtUtc);

        await _gateway
            .PushSessionAsync(_tenant.TenantId, sessionId, payload, cancellationToken)
            .ConfigureAwait(false);

        await _audit
            .RecordAsync(
                new AuditRecordRequest(
                    AuditAction.Execute,
                    "ClinicalFeedSession",
                    sessionId,
                    command.AuthenticatedUserId,
                    AuditOutcome.Success,
                    $"Session feed broadcast: eventType={eventType}.",
                    TenantId: _tenant.TenantId,
                    CorrelationId: command.CorrelationId.ToString()),
                cancellationToken)
            .ConfigureAwait(false);
    }
}
