namespace RealtimeDelivery.Application;

public static class FeedGroupNames
{
    public static string SessionGroup(string tenantId, string treatmentSessionId) =>
        $"tenant:{tenantId.Trim()}:session:{treatmentSessionId.Trim().ToUpperInvariant()}";

    public static string TenantAlertsGroup(string tenantId) =>
        $"tenant:{tenantId.Trim()}:alerts";
}
