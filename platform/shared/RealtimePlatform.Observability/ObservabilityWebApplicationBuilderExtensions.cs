using System.Reflection;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using Serilog;

namespace RealtimePlatform.Observability;

/// <summary>
/// Registers Serilog from configuration and OpenTelemetry (tracing, metrics, runtime + ASP.NET Core instrumentation).
/// </summary>
public static class ObservabilityWebApplicationBuilderExtensions
{
    /// <summary>
    /// Adds standard platform observability. Call early in host setup.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="serviceName">Resource <c>service.name</c> for OTLP exports.</param>
    public static WebApplicationBuilder AddRealtimePlatformObservability(this WebApplicationBuilder builder, string serviceName)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrWhiteSpace(serviceName);

        string deploymentEnvironment = builder.Environment.EnvironmentName;
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        string? serviceVersion = entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
            ?? entryAssembly?.GetName().Version?.ToString();

        _ = builder.Host.UseSerilog((context, services, configuration) =>
            _ = configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.With(new OpenTelemetryTraceContextEnricher()));

        _ = builder.Services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(serviceName, serviceVersion: serviceVersion)
                .AddAttributes(
                [
                    new KeyValuePair<string, object>("deployment.environment", deploymentEnvironment),
                ]))
            .WithTracing(t => t
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddOtlpExporter())
            .WithMetrics(m => m
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation()
                .AddOtlpExporter());

        return builder;
    }
}
