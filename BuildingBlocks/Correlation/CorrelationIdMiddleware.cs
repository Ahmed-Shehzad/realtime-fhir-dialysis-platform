using System.Diagnostics;

using Microsoft.AspNetCore.Http;

namespace BuildingBlocks.Correlation;

/// <summary>
/// Ensures <see cref="CorrelationIdConstants.HeaderName"/> is present on the response and stored in <see cref="HttpContext.Items"/>.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
    }

    /// <summary>
    /// Middleware entrypoint.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Ulid correlationId = ResolveCorrelationId(context.Request);
        context.Items[CorrelationIdConstants.HttpContextItemKey] = correlationId;
        context.Response.Headers.Append(CorrelationIdConstants.HeaderName, correlationId.ToString());
        Activity? activity = Activity.Current;
        if (activity is not null)
            _ = activity.SetTag("correlation.id", correlationId.ToString());

        await _next(context).ConfigureAwait(false);
    }

    private static Ulid ResolveCorrelationId(HttpRequest request)
    {
        if (request.Headers.TryGetValue(CorrelationIdConstants.HeaderName, out Microsoft.Extensions.Primitives.StringValues values)
            && values.Count > 0
            && Ulid.TryParse(values[0], out Ulid parsed))
            return parsed;

        return Ulid.NewUlid();
    }
}
