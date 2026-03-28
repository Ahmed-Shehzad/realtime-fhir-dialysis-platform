using Microsoft.AspNetCore.Authorization;

namespace RealtimePlatform.ApiGateway.Authorization;

/// <summary>
/// Gate for all YARP-forwarded requests: anonymous allowed only when running in Development with JWT bypass
/// (see <see cref="BuildingBlocks.Options.JwtBearerStartupOptions.DevelopmentBypass"/>); otherwise an authenticated
/// caller is required before the request is proxied.
/// </summary>
public sealed class GatewayIngressRequirement : IAuthorizationRequirement;
