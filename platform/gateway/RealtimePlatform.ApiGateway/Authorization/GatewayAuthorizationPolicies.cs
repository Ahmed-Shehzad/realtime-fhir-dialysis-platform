namespace RealtimePlatform.ApiGateway.Authorization;

/// <summary>Authorization policy names used only by <see cref="RealtimePlatform.ApiGateway"/>.</summary>
public static class GatewayAuthorizationPolicies
{
    /// <summary>Proxied API routes; see <see cref="GatewayIngressRequirement"/>.</summary>
    public const string Ingress = nameof(Ingress);
}
