using Microsoft.Extensions.DependencyInjection;

namespace BuildingBlocks.OpenApi;

/// <summary>
/// Registers OpenAPI with platform-wide document and operation transformers (JWT scheme, headers, Bearer <c>security</c> on authorized endpoints).
/// </summary>
public static class DialysisOpenApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAPI generation with correlation/tenant parameters, Bearer requirements on authorized operations, and JWT scheme metadata.
    /// </summary>
    public static IServiceCollection AddDialysisPlatformOpenApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddOpenApi(static options =>
        {
            _ = options.AddOperationTransformer<CorrelationAndTenantHeadersOperationTransformer>();
            _ = options.AddOperationTransformer<BearerSecurityRequirementOperationTransformer>();
            _ = options.AddDocumentTransformer<JwtBearerSecurityDocumentTransformer>();
        });
    }
}
