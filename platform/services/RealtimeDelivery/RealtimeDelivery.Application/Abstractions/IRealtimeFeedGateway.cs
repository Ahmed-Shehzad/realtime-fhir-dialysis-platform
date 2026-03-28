using RealtimeDelivery.Domain.Contracts;

namespace RealtimeDelivery.Application.Abstractions;

public interface IRealtimeFeedGateway
{
    Task PushSessionAsync(
        string tenantId,
        string treatmentSessionId,
        SessionFeedPayload payload,
        CancellationToken cancellationToken = default);

    Task PushAlertAsync(
        string tenantId,
        AlertFeedPayload payload,
        CancellationToken cancellationToken = default);
}
