using System.Diagnostics;

using Serilog.Core;
using Serilog.Events;

namespace RealtimePlatform.Observability;

/// <summary>
/// Adds <see cref="Activity.TraceId"/> and <see cref="Activity.SpanId"/> to log events for log–trace correlation.
/// </summary>
public sealed class OpenTelemetryTraceContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        Activity? activity = Activity.Current;
        if (activity is null)
            return;

        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", activity.TraceId.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("SpanId", activity.SpanId.ToString()));
    }
}
