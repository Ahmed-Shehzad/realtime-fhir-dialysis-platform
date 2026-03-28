using System.Security.Claims;

using BuildingBlocks.Options;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BuildingBlocks.Authorization;

/// <summary>
/// Handles <see cref="ScopeOrBypassRequirement"/>.
/// Succeeds when: (1) DevelopmentBypass is enabled in Development, or
/// (2) user is authenticated and has one of the required scopes in "scope" or "scp" claim.
/// </summary>
public sealed class ScopeOrBypassHandler : AuthorizationHandler<ScopeOrBypassRequirement>
{
    private readonly IHostEnvironment _env;
    private readonly IOptionsMonitor<JwtBearerStartupOptions> _jwtOptions;

    public ScopeOrBypassHandler(IHostEnvironment env, IOptionsMonitor<JwtBearerStartupOptions> jwtOptions)
    {
        _env = env;
        _jwtOptions = jwtOptions ?? throw new ArgumentNullException(nameof(jwtOptions));
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeOrBypassRequirement requirement)
    {
        if (_env.IsDevelopment() && _jwtOptions.CurrentValue.DevelopmentBypass)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!context.User.Identity?.IsAuthenticated ?? true)
            return Task.CompletedTask;

        string[] scopes = GetScopes(context.User);
        if (requirement.AllowedScopes.Any(allowed => scopes.Contains(allowed, StringComparer.OrdinalIgnoreCase))) context.Succeed(requirement);

        return Task.CompletedTask;
    }

    private static string[] GetScopes(ClaimsPrincipal user)
    {
        List<string> scopes = [];

        foreach (Claim c in user.FindAll("scope"))
            scopes.AddRange(c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        foreach (Claim c in user.FindAll("scp"))
            scopes.AddRange(c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries));

        return [.. scopes];
    }
}
