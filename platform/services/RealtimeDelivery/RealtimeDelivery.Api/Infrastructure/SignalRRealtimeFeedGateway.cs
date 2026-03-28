using Microsoft.AspNetCore.SignalR;

using RealtimeDelivery.Api.Hubs;
using RealtimeDelivery.Application;
using RealtimeDelivery.Application.Abstractions;
using RealtimeDelivery.Domain.Contracts;

namespace RealtimeDelivery.Api.Infrastructure;

public sealed class SignalRRealtimeFeedGateway : IRealtimeFeedGateway
{
    private readonly IHubContext<ClinicalFeedHub> _hub;

    public SignalRRealtimeFeedGateway(IHubContext<ClinicalFeedHub> hub) =>
        _hub = hub ?? throw new ArgumentNullException(nameof(hub));

    public async Task PushSessionAsync(
        string tenantId,
        string treatmentSessionId,
        SessionFeedPayload payload,
        CancellationToken cancellationToken = default)
    {
        string group = FeedGroupNames.SessionGroup(tenantId, treatmentSessionId);
        await _hub.Clients.Group(group).SendAsync("sessionFeed", payload, cancellationToken).ConfigureAwait(false);
    }

    public async Task PushAlertAsync(
        string tenantId,
        AlertFeedPayload payload,
        CancellationToken cancellationToken = default)
    {
        string group = FeedGroupNames.TenantAlertsGroup(tenantId);
        await _hub.Clients.Group(group).SendAsync("alertFeed", payload, cancellationToken).ConfigureAwait(false);
    }
}
