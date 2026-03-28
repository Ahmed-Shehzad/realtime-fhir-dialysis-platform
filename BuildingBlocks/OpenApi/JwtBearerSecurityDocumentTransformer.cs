using BuildingBlocks.Options;

using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace BuildingBlocks.OpenApi;

/// <summary>
/// Adds <c>components.securitySchemes.Bearer</c> (HTTP bearer, JWT) using <see cref="JwtBearerStartupOptions"/>
/// and lists configured policy scopes from <see cref="AuthorizationScopesOptions"/> (C5 discoverability).
/// </summary>
public sealed class JwtBearerSecurityDocumentTransformer : IOpenApiDocumentTransformer
{
    private readonly JwtBearerStartupOptions _jwt;
    private readonly AuthorizationScopesOptions _scopes;

    /// <summary>
    /// Creates the transformer.
    /// </summary>
    public JwtBearerSecurityDocumentTransformer(
        IOptions<JwtBearerStartupOptions> jwtOptions,
        IOptions<AuthorizationScopesOptions> scopeOptions)
    {
        _jwt = jwtOptions?.Value ?? throw new ArgumentNullException(nameof(jwtOptions));
        _scopes = scopeOptions?.Value ?? throw new ArgumentNullException(nameof(scopeOptions));
    }

    /// <inheritdoc />
    public Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>(StringComparer.Ordinal);
        var descriptionParts = new List<string>
        {
            "Microsoft Entra ID (Azure AD) JWT. Use delegated or client-credential tokens with the scopes required by each operation."
        };
        string? authority = string.IsNullOrWhiteSpace(_jwt.Authority) ? null : _jwt.Authority.Trim();
        if (authority is not null)
            descriptionParts.Add($"Authority: `{authority}`");
        string audienceDisplay = string.IsNullOrWhiteSpace(_jwt.Audience) ? "(not set)" : _jwt.Audience.Trim();
        descriptionParts.Add($"Audience: `{audienceDisplay}`");
        descriptionParts.Add(
            "In Development with `Authentication:JwtBearer:DevelopmentBypass` = true, scope policies may succeed without a token.");
        descriptionParts.Add(
            $"API scopes (configure under `{AuthorizationScopesOptions.SectionName}`): `{_scopes.DevicesRead}`, `{_scopes.DevicesWrite}`, `{_scopes.MeasurementsRead}`, `{_scopes.MeasurementsWrite}`, `{_scopes.SessionsRead}`, `{_scopes.SessionsWrite}`, `{_scopes.AuditRead}`, `{_scopes.AuditWrite}`, `{_scopes.ValidationRead}`, `{_scopes.ValidationWrite}`, `{_scopes.ConditioningRead}`, `{_scopes.ConditioningWrite}`, `{_scopes.TerminologyRead}`, `{_scopes.TerminologyWrite}`, `{_scopes.InteroperabilityRead}`, `{_scopes.InteroperabilityWrite}`, `{_scopes.ReadModelRead}`, `{_scopes.ReadModelWrite}`, `{_scopes.SurveillanceRead}`, `{_scopes.SurveillanceWrite}`, `{_scopes.DeliveryRead}`, `{_scopes.DeliveryWrite}`, `{_scopes.AnalyticsRead}`, `{_scopes.AnalyticsWrite}`, `{_scopes.ReportingRead}`, `{_scopes.ReportingWrite}`, `{_scopes.WorkflowRead}`, `{_scopes.WorkflowWrite}`, `{_scopes.ReplayRead}`, `{_scopes.ReplayWrite}`, `{_scopes.ConfigurationRead}`, `{_scopes.ConfigurationWrite}`, `{_scopes.FinancialRead}`, `{_scopes.FinancialWrite}`.");
        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = string.Join(". ", descriptionParts),
        };
        return Task.CompletedTask;
    }
}
