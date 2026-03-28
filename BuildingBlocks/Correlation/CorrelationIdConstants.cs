namespace BuildingBlocks.Correlation;

/// <summary>
/// Shared keys for correlation propagation (C5 traceability).
/// </summary>
public static class CorrelationIdConstants
{
    /// <summary>Incoming/outgoing HTTP header (ULID recommended).</summary>
    public const string HeaderName = "X-Correlation-Id";

    /// <summary><see cref="Microsoft.AspNetCore.Http.HttpContext.Items"/> key for resolved correlation id.</summary>
    public const string HttpContextItemKey = "RealtimePlatform.CorrelationId";
}
