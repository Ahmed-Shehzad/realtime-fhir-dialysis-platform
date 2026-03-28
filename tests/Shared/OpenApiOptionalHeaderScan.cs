using System.Text.Json;

using BuildingBlocks.Correlation;
using BuildingBlocks.Tenancy;

namespace Platform.IntegrationTests.Shared;

/// <summary>
/// Scans an OpenAPI 3 <c>paths</c> object for platform header parameters.
/// </summary>
public static class OpenApiOptionalHeaderScan
{
    /// <summary>
    /// Returns whether <see cref="CorrelationIdConstants.HeaderName"/> and <see cref="TenantContext.TenantIdHeader"/> appear on any operation.
    /// </summary>
    public static (bool FoundCorrelationHeader, bool FoundTenantHeader) TryFindPlatformHeaders(JsonElement paths)
    {
        bool foundCorrelation = false;
        bool foundTenant = false;
        foreach (JsonProperty pathProp in paths.EnumerateObject())
            foreach (JsonProperty methodProp in pathProp.Value.EnumerateObject())
                AccumulateFromOperation(methodProp.Value, ref foundCorrelation, ref foundTenant);

        return (foundCorrelation, foundTenant);
    }

    private static void AccumulateFromOperation(JsonElement operation, ref bool foundCorrelation, ref bool foundTenant)
    {
        if (!operation.TryGetProperty("parameters", out JsonElement parameters))
            return;
        foreach (JsonElement param in parameters.EnumerateArray())
        {
            if (!TryGetHeaderName(param, out string? name))
                continue;
            if (string.Equals(name, CorrelationIdConstants.HeaderName, StringComparison.Ordinal))
                foundCorrelation = true;
            if (string.Equals(name, TenantContext.TenantIdHeader, StringComparison.Ordinal))
                foundTenant = true;
        }
    }

    private static bool TryGetHeaderName(JsonElement param, out string? name)
    {
        name = null;
        if (!param.TryGetProperty("in", out JsonElement inn)
            || !string.Equals(inn.GetString(), "header", StringComparison.Ordinal)
            || !param.TryGetProperty("name", out JsonElement nameEl))
            return false;
        name = nameEl.GetString();
        return true;
    }
}
