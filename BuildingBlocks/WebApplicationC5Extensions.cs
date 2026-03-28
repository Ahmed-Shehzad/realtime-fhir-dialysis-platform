using BuildingBlocks.Correlation;
using BuildingBlocks.Tenancy;

using Microsoft.AspNetCore.Builder;

namespace BuildingBlocks;

/// <summary>
/// C5 HTTP pipeline: correlation id first, then authentication and authorization.
/// </summary>
public static class WebApplicationC5Extensions
{
    /// <summary>
    /// Registers <see cref="CorrelationIdMiddleware"/>, JWT authentication, and authorization.
    /// Call before <c>MapControllers</c> / <c>MapOpenApi</c>.
    /// </summary>
    public static WebApplication UseDialysisPlatformC5(this WebApplication app)
    {
        ArgumentNullException.ThrowIfNull(app);
        _ = app.UseMiddleware<CorrelationIdMiddleware>();
        _ = app.UseTenantResolution();
        _ = app.UseAuthentication();
        _ = app.UseAuthorization();
        return app;
    }
}
