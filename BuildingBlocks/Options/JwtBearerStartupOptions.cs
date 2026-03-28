namespace BuildingBlocks.Options;

/// <summary>
/// Options for Entra ID (Azure AD) JWT bearer and development bypass (see <see cref="Authorization.ScopeOrBypassHandler"/>).
/// </summary>
public sealed class JwtBearerStartupOptions
{
    public const string SectionName = "Authentication:JwtBearer";

    /// <summary>Metadata issuer (e.g. <c>https://login.microsoftonline.com/{tenantId}/v2.0</c>).</summary>
    public string? Authority { get; set; }

    /// <summary>Application (API) audience / Application ID URI.</summary>
    public string Audience { get; set; } = "api://dialysis-pdms";

    /// <summary>When true and host environment is Development, scope policies succeed without a token (local only).</summary>
    public bool DevelopmentBypass { get; set; }
}
