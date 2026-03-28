namespace Simulation.GatewayCli;

/// <summary>
/// Relative URLs under the gateway base (see RealtimePlatform.ApiGateway ReverseProxy routes and service controllers).
/// </summary>
internal static class GatewayApiPaths
{
    internal static string Devices(string apiVersion) => $"api/v{apiVersion}/devices";

    /// <summary>GET devices/{deviceId}/trust (DeviceRegistry.Api).</summary>
    internal static string DeviceTrust(string apiVersion, string deviceId) =>
        $"{Devices(apiVersion)}/{Uri.EscapeDataString(deviceId)}/trust";

    internal static string Sessions(string apiVersion) => $"api/v{apiVersion}/sessions";

    internal static string SessionPatient(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/patient";

    internal static string SessionDevice(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/device";

    internal static string SessionStart(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/start";

    internal static string SessionComplete(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/complete";

    internal static string Measurements(string apiVersion) => $"api/v{apiVersion}/measurements";

    internal static string DeliveryBroadcastSession(string apiVersion) =>
        $"api/v{apiVersion}/delivery/broadcast/session";

    internal static string DeliveryBroadcastAlert(string apiVersion) =>
        $"api/v{apiVersion}/delivery/broadcast/alert";

    internal static string ProjectionsAlerts(string apiVersion) => $"api/v{apiVersion}/projections/alerts";

    internal static string ProjectionsSessionOverview(string apiVersion) =>
        $"api/v{apiVersion}/projections/session-overview";
}
