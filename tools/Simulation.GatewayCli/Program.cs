using System.Text.Json;

namespace Simulation.GatewayCli;

internal static class Program
{
    /// <summary>Entry point. Process arguments are ignored; use environment variables (see README).</summary>
    public async static Task<int> Main(string[] args)
    {
        _ = args;

        GlobalOptions? globals = null;
        CancellationTokenSource? sessionCts = null;
        try
        {
            globals = GlobalOptions.FromEnvironment();

            sessionCts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                sessionCts?.Cancel();
            };

            string prefix = ReadIngestPrefix();
            return await RunComprehensiveIngestAsync(
                    globals,
                    prefix,
                    sessionCts.Token)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (sessionCts?.IsCancellationRequested == true)
        {
            await Console.Error.WriteLineAsync("Canceled (Ctrl+C).").ConfigureAwait(false);
            return 130;
        }
        catch (OperationCanceledException ex)
        {
            await WriteHttpTimeoutHintAsync(ex, globals).ConfigureAwait(false);
            return 1;
        }
        catch (TimeoutException ex)
        {
            await WriteHttpTimeoutHintAsync(ex, globals).ConfigureAwait(false);
            return 1;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync(ex.Message).ConfigureAwait(false);
            return 1;
        }
        finally
        {
            sessionCts?.Dispose();
        }
    }

    private static string ReadIngestPrefix() =>
        FirstNonEmptyEnvironment("SIMULATION_GATEWAY_INGEST_PREFIX", "SIMULATION_SCENARIO_PREFIX") ?? "sim";

    private static string? FirstNonEmptyEnvironment(params string[] names)
    {
        foreach (string name in names)
        {
            string? v = Environment.GetEnvironmentVariable(name)?.Trim();
            if (string.IsNullOrEmpty(v)) continue;

            return v;
        }

        return null;
    }

    private async static Task<int> RunComprehensiveIngestAsync(
        GlobalOptions g,
        string prefix,
        CancellationToken cancellationToken)
    {
        ComprehensiveRelationalIngest.Summary summary =
            await ComprehensiveRelationalIngest.RunAsync(
                    g,
                    prefix,
                    skipReadModelProjections: false,
                    skipFinancialChain: false,
                    skipProjectionRebuild: false,
                    skipReplayRecovery: false,
                    cancellationToken)
                .ConfigureAwait(false);
        Console.WriteLine(JsonSerializer.Serialize(summary, GatewayHttp.JsonWriteOptions));
        return 0;
    }

    private async static Task WriteHttpTimeoutHintAsync(Exception ex, GlobalOptions? globals)
    {
        TimeSpan limit = globals?.HttpClientTimeout ?? TimeSpan.FromSeconds(GlobalOptions.DefaultHttpClientTimeoutSeconds);
        string gateway = globals?.GatewayBase?.Trim() is { Length: > 0 } g ? g : "(set SIMULATION_GATEWAY_BASE)";
        await Console.Error.WriteLineAsync(
                "HTTP call did not finish in time or the connection stalled before the first response bytes. "
                + $"HttpClient timeout is {limit.TotalSeconds:0} s; gateway base is {gateway}. "
                + "Check that RealtimePlatform.ApiGateway is running, the URL is correct, and downstream services or databases are reachable (YARP can wait on slow health checks). "
                + "For device register, the gateway forwards to DeviceRegistry.Api (RealtimePlatform.ApiGateway ReverseProxy cluster device-registry; default destination http://localhost:5001/). "
                + "If that service is up but still hangs, confirm Postgres matches DeviceRegistry.Api ConnectionStrings:Default (e.g. Host for your run: Docker vs host dotnet). "
                + "Increase SIMULATION_GATEWAY_TIMEOUT_SECONDS for slow cold starts. "
                + $"Detail: {ex.Message}")
            .ConfigureAwait(false);
    }
}

internal sealed record GlobalOptions(
    string GatewayBase,
    string? TenantId,
    string? BearerToken,
    string? CorrelationId,
    string ApiVersion,
    TimeSpan HttpClientTimeout,
    bool TraceHttp)
{
    internal const int DefaultHttpClientTimeoutSeconds = 120;

    internal static GlobalOptions FromEnvironment()
    {
        string gateway = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_BASE") ?? "http://localhost:5100";
        string? tenant = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_TENANT")
            ?? Environment.GetEnvironmentVariable("SIMULATION_TENANT");
        string? token = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_BEARER_TOKEN");
        string? correlation = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_CORRELATION_ID");
        string apiVersion = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_API_VERSION")?.Trim() is { Length: > 0 } v ? v : "1";
        int timeoutSeconds = DefaultHttpClientTimeoutSeconds;
        bool traceHttp = IsTruthyEnvironmentVariable("SIMULATION_GATEWAY_VERBOSE");
        string? timeoutEnv = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_TIMEOUT_SECONDS");
        if (!string.IsNullOrWhiteSpace(timeoutEnv)
            && int.TryParse(timeoutEnv.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int fromEnv)
            && fromEnv > 0) timeoutSeconds = fromEnv;

        return new GlobalOptions(
            gateway,
            tenant,
            token,
            correlation,
            apiVersion,
            TimeSpan.FromSeconds(timeoutSeconds),
            traceHttp);
    }

    internal static bool IsTruthyEnvironmentVariable(string name)
    {
        string? v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(v)) return false;
        v = v.Trim();
        return string.Equals(v, "1", StringComparison.Ordinal)
            || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase);
    }
}
