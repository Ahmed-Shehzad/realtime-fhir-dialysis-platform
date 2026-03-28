using System.Text.Json;

namespace Platform.IntegrationTests.Shared;

/// <summary>
/// Asserts OpenAPI 3 <c>components.securitySchemes</c> contains HTTP bearer JWT.
/// </summary>
public static class OpenApiBearerSecurityScan
{
    /// <summary>
    /// Returns true when <c>Bearer</c> is an HTTP bearer scheme (JWT).
    /// </summary>
    public static bool HasBearerJwtSecurityScheme(JsonElement documentRoot)
    {
        if (!documentRoot.TryGetProperty("components", out JsonElement components))
            return false;
        if (!components.TryGetProperty("securitySchemes", out JsonElement schemes))
            return false;
        if (!schemes.TryGetProperty("Bearer", out JsonElement bearer))
            return false;
        if (!bearer.TryGetProperty("type", out JsonElement typeEl))
            return false;
        if (!string.Equals(typeEl.GetString(), "http", StringComparison.OrdinalIgnoreCase))
            return false;
        if (!bearer.TryGetProperty("scheme", out JsonElement schemeEl))
            return false;
        return string.Equals(schemeEl.GetString(), "bearer", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Returns true when the Bearer scheme description includes the configured scope strings (sanity check for OpenAPI metadata).
    /// </summary>
    public static bool BearerDescriptionListsAuthorizationScopes(JsonElement documentRoot)
    {
        if (!TryGetBearerDescription(documentRoot, out string? description)
            || string.IsNullOrEmpty(description))
            return false;
        return description.Contains("Dialysis.Devices.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Measurements.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Sessions.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Audit.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Validation.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Conditioning.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Terminology.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Interoperability.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.ReadModel.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.ReadModel.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Surveillance.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Surveillance.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Delivery.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Delivery.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Analytics.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Analytics.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Reporting.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Reporting.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Workflow.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Workflow.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Replay.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Replay.Write", StringComparison.Ordinal)
            && description.Contains("Dialysis.Configuration.Read", StringComparison.Ordinal)
            && description.Contains("Dialysis.Configuration.Write", StringComparison.Ordinal);
    }

    /// <summary>
    /// Returns true when at least one path operation lists a Bearer security requirement (OpenAPI 3.x may use the key
    /// <c>Bearer</c> or a JSON reference path ending in <c>securitySchemes/Bearer</c>).
    /// </summary>
    public static bool AnyOperationRequiresBearerSecurity(JsonElement paths)
    {
        foreach (JsonProperty pathProp in paths.EnumerateObject())
            foreach (JsonProperty methodProp in pathProp.Value.EnumerateObject())
                if (OperationRequiresBearer(methodProp.Value))
                    return true;

        return false;
    }

    private static bool OperationRequiresBearer(JsonElement operation)
    {
        if (!operation.TryGetProperty("security", out JsonElement security))
            return false;
        foreach (JsonElement requirement in security.EnumerateArray())
            if (SecurityRequirementReferencesBearer(requirement))
                return true;

        return false;
    }

    private static bool SecurityRequirementReferencesBearer(JsonElement requirement)
    {
        if (requirement.ValueKind != JsonValueKind.Object)
            return false;
        foreach (JsonProperty secEntry in requirement.EnumerateObject())
            if (secEntry.Name.Contains("Bearer", StringComparison.Ordinal))
                return true;

        return false;
    }

    /// <summary>
    /// Returns true when some operation's Bearer security requirement lists <paramref name="scopeValue"/> as a scope string.
    /// </summary>
    public static bool AnyBearerRequirementListsScope(JsonElement paths, string scopeValue)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(scopeValue);
        foreach (JsonProperty pathProp in paths.EnumerateObject())
            foreach (JsonProperty methodProp in pathProp.Value.EnumerateObject())
                if (OperationSecurityListsScope(methodProp.Value, scopeValue))
                    return true;

        return false;
    }

    private static bool OperationSecurityListsScope(JsonElement operation, string scopeValue)
    {
        if (!operation.TryGetProperty("security", out JsonElement security))
            return false;
        foreach (JsonElement requirement in security.EnumerateArray())
            if (SecurityRequirementListsScope(requirement, scopeValue))
                return true;

        return false;
    }

    private static bool SecurityRequirementListsScope(JsonElement requirement, string scopeValue)
    {
        if (requirement.ValueKind != JsonValueKind.Object)
            return false;
        foreach (JsonProperty secEntry in requirement.EnumerateObject())
        {
            if (secEntry.Value.ValueKind != JsonValueKind.Array)
                continue;
            foreach (JsonElement scopeEl in secEntry.Value.EnumerateArray())
                if (scopeEl.ValueKind == JsonValueKind.String
                    && string.Equals(scopeEl.GetString(), scopeValue, StringComparison.Ordinal))
                    return true;
        }

        return false;
    }

    private static bool TryGetBearerDescription(JsonElement documentRoot, out string? description)
    {
        description = null;
        if (!documentRoot.TryGetProperty("components", out JsonElement components))
            return false;
        if (!components.TryGetProperty("securitySchemes", out JsonElement schemes))
            return false;
        if (!schemes.TryGetProperty("Bearer", out JsonElement bearer))
            return false;
        if (!bearer.TryGetProperty("description", out JsonElement descEl))
            return false;
        description = descEl.GetString();
        return description is not null;
    }
}
