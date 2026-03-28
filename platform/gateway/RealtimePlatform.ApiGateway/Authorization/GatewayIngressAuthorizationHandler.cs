using BuildingBlocks.Options;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace RealtimePlatform.ApiGateway.Authorization;

/// <summary>
/// Satisfies <see cref="GatewayIngressRequirement"/> when Development JWT bypass is active or the user is authenticated.
/// </summary>
public sealed class GatewayIngressAuthorizationHandler : AuthorizationHandler<GatewayIngressRequirement>
{
    private readonly IHostEnvironment _environment;
    private readonly IOptionsMonitor<JwtBearerStartupOptions> _jwtOptions;

    public GatewayIngressAuthorizationHandler(
        IHostEnvironment environment,
        IOptionsMonitor<JwtBearerStartupOptions> jwtOptions)
    {
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        GatewayIngressRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (_environment.IsDevelopment() && _jwtOptions.CurrentValue.DevelopmentBypass)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (context.User.Identity?.IsAuthenticated == true) context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
