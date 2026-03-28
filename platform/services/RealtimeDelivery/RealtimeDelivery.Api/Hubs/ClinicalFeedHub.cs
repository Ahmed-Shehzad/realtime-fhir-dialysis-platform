using BuildingBlocks.Authorization;
using BuildingBlocks.Tenancy;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

using RealtimeDelivery.Application;

namespace RealtimeDelivery.Api.Hubs;

[Authorize(Policy = PlatformAuthorizationPolicies.DeliveryRead)]
public sealed class ClinicalFeedHub : Hub
{
    private readonly ITenantContext _tenant;

    public ClinicalFeedHub(ITenantContext tenant) =>
        _tenant = tenant ?? throw new ArgumentNullException(nameof(tenant));

    [HubMethodName("JoinSessionFeed")]
    public async Task JoinSessionFeedAsync(string treatmentSessionId)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            throw new HubException("TreatmentSessionId is required.");

        string group = FeedGroupNames.SessionGroup(_tenant.TenantId, treatmentSessionId.Trim());
        await Groups.AddToGroupAsync(Context.ConnectionId, group).ConfigureAwait(false);
    }

    [HubMethodName("LeaveSessionFeed")]
    public async Task LeaveSessionFeedAsync(string treatmentSessionId)
    {
        if (string.IsNullOrWhiteSpace(treatmentSessionId))
            return;

        string group = FeedGroupNames.SessionGroup(_tenant.TenantId, treatmentSessionId.Trim());
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group).ConfigureAwait(false);
    }

    [HubMethodName("JoinTenantAlerts")]
    public async Task JoinTenantAlertsAsync()
    {
        string group = FeedGroupNames.TenantAlertsGroup(_tenant.TenantId);
        await Groups.AddToGroupAsync(Context.ConnectionId, group).ConfigureAwait(false);
    }

    [HubMethodName("LeaveTenantAlerts")]
    public async Task LeaveTenantAlertsAsync()
    {
        string group = FeedGroupNames.TenantAlertsGroup(_tenant.TenantId);
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group).ConfigureAwait(false);
    }
}
