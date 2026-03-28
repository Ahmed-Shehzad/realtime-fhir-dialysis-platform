using BuildingBlocks.Audit;
using BuildingBlocks.Authorization;
using BuildingBlocks.Correlation;
using BuildingBlocks.Options;
using BuildingBlocks.Tenancy;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BuildingBlocks.DependencyInjection;

/// <summary>
/// C5-oriented authentication (Entra ID JWT), scope policies, correlation id access, and structured audit recorder registration.
/// </summary>
public static class C5WebApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds JWT bearer authentication, <see cref="PlatformAuthorizationPolicies"/>, HTTP correlation access, and audit recorder registration.
    /// </summary>
    public static IServiceCollection AddDialysisPlatformC5WebApi(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        _ = services.AddJwtBearerStartupValidation(configuration);
        _ = services.Configure<AuthorizationScopesOptions>(
            configuration.GetSection(AuthorizationScopesOptions.SectionName));
        _ = services.AddSingleton<IAuthorizationHandler, ScopeOrBypassHandler>();
        _ = services.AddSingleton<IConfigureOptions<AuthorizationOptions>, ConfigureDialysisAuthorizationOptions>();

        _ = services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                IConfigurationSection jwt = configuration.GetSection(JwtBearerStartupOptions.SectionName);
                string? authority = jwt[nameof(JwtBearerStartupOptions.Authority)];
                string? audience = jwt[nameof(JwtBearerStartupOptions.Audience)];
                options.Authority = string.IsNullOrWhiteSpace(authority) ? null : authority.Trim();
                options.Audience = string.IsNullOrWhiteSpace(audience) ? null : audience.Trim();
                options.MapInboundClaims = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = !string.IsNullOrWhiteSpace(options.Audience),
                    ValidateIssuer = !string.IsNullOrWhiteSpace(options.Authority),
                };
            });

        _ = services.AddAuthorization();
        _ = services.AddHttpContextAccessor();
        _ = services.AddScoped<ICorrelationIdAccessor, CorrelationIdAccessor>();
        _ = services.AddTenantResolution();
        _ = services.AddAuditRecorder();
        return services;
    }
}
