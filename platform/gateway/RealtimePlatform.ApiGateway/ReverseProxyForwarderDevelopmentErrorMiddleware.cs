using Yarp.ReverseProxy.Forwarder;

namespace RealtimePlatform.ApiGateway;

/// <summary>
/// In Development, attaches a JSON body to YARP forwarder failures when the response has not started,
/// so clients see why the gateway returned 502/503 instead of an empty body.
/// </summary>
public sealed class ReverseProxyForwarderDevelopmentErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<ReverseProxyForwarderDevelopmentErrorMiddleware> _logger;

    public ReverseProxyForwarderDevelopmentErrorMiddleware(
        RequestDelegate next,
        IWebHostEnvironment environment,
        ILogger<ReverseProxyForwarderDevelopmentErrorMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        await _next(context).ConfigureAwait(false);

        if (!_environment.IsDevelopment()) return;

        IForwarderErrorFeature? forwarderError = context.GetForwarderErrorFeature();
        if (forwarderError is null) return;

        if (context.Response.HasStarted)
        {
            _logger.LogDebug(
                "YARP forwarder error {ForwarderError} but response already started; skipping JSON body.",
                forwarderError.Error);
            return;
        }

        int originalStatus = context.Response.StatusCode;
        context.Response.Clear();
        context.Response.StatusCode = originalStatus is >= StatusCodes.Status400BadRequest and <= 599
            ? originalStatus
            : StatusCodes.Status502BadGateway;
        context.Response.ContentType = "application/json; charset=utf-8";

        string? clusterId = context.GetReverseProxyFeature()?.Route?.Cluster?.ClusterId;
        string hint = clusterId switch
        {
            "device-registry" =>
                "Start DeviceRegistry.Api (default http://localhost:5001) and ensure PostgreSQL is reachable for ConnectionStrings:Default.",
            _ =>
                "Start the upstream API for this cluster (see RealtimePlatform.ApiGateway appsettings ReverseProxy:Clusters).",
        };

        await context.Response.WriteAsJsonAsync(
                new
                {
                    title = "Reverse proxy upstream error",
                    forwarderError = forwarderError.Error.ToString(),
                    clusterId,
                    exceptionType = forwarderError.Exception?.GetType().FullName,
                    exceptionMessage = forwarderError.Exception?.Message,
                    hint,
                })
            .ConfigureAwait(false);
    }
}
