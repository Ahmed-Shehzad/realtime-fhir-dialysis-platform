using System.Text.Json;

namespace Simulation.GatewayCli;

internal static class Program
{
    public async static Task<int> Main(string[] args)
    {
        GlobalOptions? globals = null;
        CancellationTokenSource? sessionCts = null;
        try
        {
            if (args.Length == 0 || args.Contains("-h", StringComparer.Ordinal) || args.Contains("--help", StringComparer.Ordinal))
            {
                PrintHelp();
                return args.Length == 0 ? 1 : 0;
            }

            var argv = args.ToList();
            globals = GlobalOptions.Parse(argv);
            if (argv.Count == 0)
            {
                PrintHelp();
                return 1;
            }

            string verb = argv[0];
            argv.RemoveAt(0);

            sessionCts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                sessionCts?.Cancel();
            };

            return verb switch
            {
                "device" => await HandleDeviceAsync(globals, argv, sessionCts.Token).ConfigureAwait(false),
                "session" => await HandleSessionAsync(globals, argv, sessionCts.Token).ConfigureAwait(false),
                "measurement" => await HandleMeasurementAsync(globals, argv, sessionCts.Token).ConfigureAwait(false),
                "broadcast" => await HandleBroadcastAsync(globals, argv, sessionCts.Token).ConfigureAwait(false),
                "projection" => await HandleProjectionAsync(globals, argv, sessionCts.Token).ConfigureAwait(false),
                "scenario" => await HandleScenarioAsync(globals, argv, sessionCts.Token).ConfigureAwait(false),
                _ => Unknown(verb),
            };
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

    private async static Task WriteHttpTimeoutHintAsync(Exception ex, GlobalOptions? globals)
    {
        TimeSpan limit = globals?.HttpClientTimeout ?? TimeSpan.FromSeconds(GlobalOptions.DefaultHttpClientTimeoutSeconds);
        string gateway = globals?.GatewayBase?.Trim() is { Length: > 0 } g ? g : "(set --gateway or SIMULATION_GATEWAY_BASE)";
        await Console.Error.WriteLineAsync(
                "HTTP call did not finish in time or the connection stalled before the first response bytes. "
                + $"HttpClient timeout is {limit.TotalSeconds:0} s; gateway base is {gateway}. "
                + "Check that RealtimePlatform.ApiGateway is running, the URL is correct, and downstream services or databases are reachable (YARP can wait on slow health checks). "
                + "For device register, the gateway forwards to DeviceRegistry.Api (RealtimePlatform.ApiGateway ReverseProxy cluster device-registry; default destination http://localhost:5001/). "
                + "If that service is up but still hangs, confirm Postgres matches DeviceRegistry.Api ConnectionStrings:Default (e.g. Host for your run: Docker vs host dotnet). "
                + "Increase --timeout-seconds or SIMULATION_GATEWAY_TIMEOUT_SECONDS for slow cold starts. "
                + $"Detail: {ex.Message}")
            .ConfigureAwait(false);
    }

    private static int Unknown(string verb)
    {
        Console.Error.WriteLine($"Unknown command: {verb}");
        return 1;
    }

    private static void PrintHelp()
    {
        Console.WriteLine(
            $"""
            simulate-gateway — POST through RealtimePlatform.ApiGateway (local / staging).

            Global options (any order before subcommand):
              --gateway URL        Default: SIMULATION_GATEWAY_BASE or http://localhost:5100
              --tenant ID          X-Tenant-Id (default: SIMULATION_GATEWAY_TENANT)
              --token JWT          Bearer (default: SIMULATION_GATEWAY_BEARER_TOKEN)
              --correlation-id ID  Optional X-Correlation-Id
              --api-version N      Path segment (default: 1)
              --timeout-seconds N  HttpClient.Timeout (default: SIMULATION_GATEWAY_TIMEOUT_SECONDS or {GlobalOptions.DefaultHttpClientTimeoutSeconds})
              --verbose, -v         Log each request URL to stderr (or SIMULATION_GATEWAY_VERBOSE=1)

            Commands:
              device register --identifier ID [--manufacturer M] [--trust Active|Suspended|Retired]
              device trust --device-id ID   (GET trust via gateway)
              session create
              session assign-patient --session-id ULID --mrn MRN
              session link-device --session-id ULID --device-id IDENT
              session start --session-id ULID
              session complete --session-id ULID
              measurement ingest --device ID --channel C --type T --schema-version V --payload-json JSON
              broadcast session --session-id ULID --event-type TYPE --summary TEXT [--when ISO8601]
              broadcast alert [--session-id ULID] --alert-id ID --severity S --state STATE --event-type TYPE [--when ISO8601]
              projection upsert-alert --row-key KEY --alert-type TYPE --severity S --alert-state STATE [--session-id ULID] [--when ISO8601]
              projection upsert-session-overview --session-id ULID --session-state STATE [--patient-label L] [--device-id D] [--started-at ISO8601]
              scenario run [--prefix PREFIX] [--skip-read-model]  (stderr progress; stdout JSON; SignalR + optional read-model upserts)

            See tools/Simulation.GatewayCli/README.md for env vars and prerequisites.
            """);
    }

    private async static Task<int> HandleDeviceAsync(GlobalOptions g, List<string> argv, CancellationToken cancellationToken)
    {
        if (argv.Count < 1)
        {
            Console.Error.WriteLine("Usage: device register --identifier ID ... | device trust --device-id ID");
            return 1;
        }

        string sub = argv[0];
        argv.RemoveAt(0);

        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(baseUri, g.TenantId, g.BearerToken, g.CorrelationId, g.HttpClientTimeout);

        if (string.Equals(sub, "register", StringComparison.OrdinalIgnoreCase))
        {
            var parsedReg = KeyedOptions.Parse(argv, required: new[] { "--identifier" });
            string identifier = parsedReg.Required("--identifier");
            string manufacturer = parsedReg.Optional("--manufacturer") ?? "Simulation";
            string trust = parsedReg.Optional("--trust") ?? "Active";
            parsedReg.ExpectNoRemainingPositional();

            var body = new { deviceIdentifier = identifier, manufacturer, initialTrustState = trust };
            HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.Devices(g.ApiVersion),
                    body,
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
            await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        if (string.Equals(sub, "trust", StringComparison.OrdinalIgnoreCase))
        {
            var parsedTrust = KeyedOptions.Parse(argv, required: new[] { "--device-id" });
            string deviceId = parsedTrust.Required("--device-id");
            parsedTrust.ExpectNoRemainingPositional();

            HttpResponseMessage response = await GatewayHttp.GetAsync(
                    client,
                    GatewayApiPaths.DeviceTrust(g.ApiVersion, deviceId),
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
            await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        Console.Error.WriteLine("Usage: device register --identifier ID ... | device trust --device-id ID");
        return 1;
    }

    private async static Task<int> HandleSessionAsync(GlobalOptions g, List<string> argv, CancellationToken cancellationToken)
    {
        if (argv.Count < 1)
        {
            Console.Error.WriteLine("Usage: session create | assign-patient | link-device | start | complete ...");
            return 1;
        }

        string sub = argv[0];
        argv.RemoveAt(0);
        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(baseUri, g.TenantId, g.BearerToken, g.CorrelationId, g.HttpClientTimeout);
        string v = g.ApiVersion;

        switch (sub)
        {
            case "create":
            {
                using var msg = new HttpRequestMessage(HttpMethod.Post, GatewayApiPaths.Sessions(v));
                GatewayHttp.TraceHttpRequest(client, msg, g.TraceHttp);
                HttpResponseMessage response = await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
                await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            case "assign-patient":
            {
                var parsed = KeyedOptions.Parse(argv, required: new[] { "--session-id", "--mrn" });
                string sessionId = parsed.Required("--session-id");
                string mrn = parsed.Required("--mrn");
                parsed.ExpectNoRemainingPositional();
                var body = new { medicalRecordNumber = mrn };
                HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.SessionPatient(v, sessionId),
                        body,
                        cancellationToken,
                        g.TraceHttp)
                    .ConfigureAwait(false);
                await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            case "link-device":
            {
                var parsed = KeyedOptions.Parse(argv, required: new[] { "--session-id", "--device-id" });
                string sessionId = parsed.Required("--session-id");
                string deviceIdent = parsed.Required("--device-id");
                parsed.ExpectNoRemainingPositional();
                var body = new { deviceIdentifier = deviceIdent };
                HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.SessionDevice(v, sessionId),
                        body,
                        cancellationToken,
                        g.TraceHttp)
                    .ConfigureAwait(false);
                await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            case "start":
            {
                var parsed = KeyedOptions.Parse(argv, required: new[] { "--session-id" });
                string sessionId = parsed.Required("--session-id");
                parsed.ExpectNoRemainingPositional();
                using var msg = new HttpRequestMessage(
                    HttpMethod.Post,
                    GatewayApiPaths.SessionStart(v, sessionId));
                GatewayHttp.TraceHttpRequest(client, msg, g.TraceHttp);
                HttpResponseMessage response = await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
                await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            case "complete":
            {
                var parsed = KeyedOptions.Parse(argv, required: new[] { "--session-id" });
                string sessionId = parsed.Required("--session-id");
                parsed.ExpectNoRemainingPositional();
                using var msg = new HttpRequestMessage(
                    HttpMethod.Post,
                    GatewayApiPaths.SessionComplete(v, sessionId));
                GatewayHttp.TraceHttpRequest(client, msg, g.TraceHttp);
                HttpResponseMessage response = await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
                await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
                return 0;
            }

            default:
                Console.Error.WriteLine($"Unknown session subcommand: {sub}");
                return 1;
        }
    }

    private async static Task<int> HandleMeasurementAsync(GlobalOptions g, List<string> argv, CancellationToken cancellationToken)
    {
        if (argv.Count < 2 || argv[0] != "ingest")
        {
            Console.Error.WriteLine(
                "Usage: measurement ingest --device ID --channel C --type T --schema-version V --payload-json '{...}'");
            return 1;
        }

        argv.RemoveAt(0);
        var parsed = KeyedOptions.Parse(
            argv,
            required: new[] { "--device", "--channel", "--type", "--schema-version", "--payload-json" });
        string device = parsed.Required("--device");
        string channel = parsed.Required("--channel");
        string type = parsed.Required("--type");
        string schema = parsed.Required("--schema-version");
        string rawJson = parsed.Required("--payload-json");
        parsed.ExpectNoRemainingPositional();

        if (rawJson.Length == 0 || !IsValidJsonObject(rawJson))
        {
            Console.Error.WriteLine("--payload-json must be a JSON object or array string.");
            return 1;
        }

        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(baseUri, g.TenantId, g.BearerToken, g.CorrelationId, g.HttpClientTimeout);
        var body = new
        {
            deviceIdentifier = device,
            channel = channel,
            measurementType = type,
            schemaVersion = schema,
            rawPayloadJson = rawJson,
        };
        HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                client,
                GatewayApiPaths.Measurements(g.ApiVersion),
                body,
                cancellationToken,
                g.TraceHttp)
            .ConfigureAwait(false);
        await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
        return 0;
    }

    private static bool IsValidJsonObject(string raw)
    {
        try
        {
            using JsonDocument doc = JsonDocument.Parse(raw);
            return doc.RootElement.ValueKind is JsonValueKind.Object or JsonValueKind.Array;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private async static Task<int> HandleBroadcastAsync(GlobalOptions g, List<string> argv, CancellationToken cancellationToken)
    {
        if (argv.Count < 1)
        {
            Console.Error.WriteLine("Usage: broadcast session | alert ...");
            return 1;
        }

        string sub = argv[0];
        argv.RemoveAt(0);
        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(baseUri, g.TenantId, g.BearerToken, g.CorrelationId, g.HttpClientTimeout);
        string v = g.ApiVersion;

        if (sub == "session")
        {
            var parsed = KeyedOptions.Parse(argv, required: new[] { "--session-id", "--event-type", "--summary" });
            string sessionId = parsed.Required("--session-id");
            string eventType = parsed.Required("--event-type");
            string summary = parsed.Required("--summary");
            DateTimeOffset when = ParseWhen(parsed.Optional("--when"));
            parsed.ExpectNoRemainingPositional();
            var body = new { treatmentSessionId = sessionId, eventType, summary, occurredAtUtc = when };
            HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.DeliveryBroadcastSession(v),
                    body,
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
            await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        if (sub == "alert")
        {
            var parsed = KeyedOptions.Parse(
                argv,
                required: new[] { "--alert-id", "--severity", "--state", "--event-type" });
            string? sessionId = parsed.Optional("--session-id");
            string alertId = parsed.Required("--alert-id");
            string severity = parsed.Required("--severity");
            string state = parsed.Required("--state");
            string eventType = parsed.Required("--event-type");
            DateTimeOffset when = ParseWhen(parsed.Optional("--when"));
            parsed.ExpectNoRemainingPositional();
            var body = new
            {
                eventType,
                treatmentSessionId = sessionId,
                alertId,
                severity,
                lifecycleState = state,
                occurredAtUtc = when,
            };
            HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.DeliveryBroadcastAlert(v),
                    body,
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
            await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        Console.Error.WriteLine($"Unknown broadcast subcommand: {sub}");
        return 1;
    }

    private async static Task<int> HandleProjectionAsync(GlobalOptions g, List<string> argv, CancellationToken cancellationToken)
    {
        if (argv.Count < 1)
        {
            Console.Error.WriteLine("Usage: projection upsert-alert ... | upsert-session-overview ...");
            return 1;
        }

        string sub = argv[0];
        argv.RemoveAt(0);
        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(baseUri, g.TenantId, g.BearerToken, g.CorrelationId, g.HttpClientTimeout);
        string v = g.ApiVersion;

        if (sub == "upsert-alert")
        {
            var parsed = KeyedOptions.Parse(
                argv,
                required: new[] { "--row-key", "--alert-type", "--severity", "--alert-state" });
            string rowKey = parsed.Required("--row-key");
            string alertType = parsed.Required("--alert-type");
            string severity = parsed.Required("--severity");
            string alertState = parsed.Required("--alert-state");
            string? sessionId = parsed.Optional("--session-id");
            DateTimeOffset when = ParseWhen(parsed.Optional("--when"));
            parsed.ExpectNoRemainingPositional();
            var body = new
            {
                alertRowKey = rowKey,
                alertType,
                severity,
                alertState,
                treatmentSessionId = sessionId,
                raisedAtUtc = when,
            };
            HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.ProjectionsAlerts(v),
                    body,
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
            await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        if (sub == "upsert-session-overview")
        {
            var parsed = KeyedOptions.Parse(argv, required: new[] { "--session-id", "--session-state" });
            string sessionId = parsed.Required("--session-id");
            string sessionState = parsed.Required("--session-state");
            string? patientLabel = parsed.Optional("--patient-label");
            string? deviceId = parsed.Optional("--device-id");
            DateTimeOffset started = ParseWhen(parsed.Optional("--started-at"));
            parsed.ExpectNoRemainingPositional();
            var body = new
            {
                treatmentSessionId = sessionId,
                sessionState,
                patientDisplayLabel = patientLabel,
                linkedDeviceId = deviceId,
                sessionStartedAtUtc = started,
            };
            HttpResponseMessage response = await GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.ProjectionsSessionOverview(v),
                    body,
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
            await GatewayHttp.WriteResultAsync(response, cancellationToken).ConfigureAwait(false);
            return 0;
        }

        Console.Error.WriteLine($"Unknown projection subcommand: {sub}");
        return 1;
    }

    private static DateTimeOffset ParseWhen(string? iso)
    {
        if (string.IsNullOrWhiteSpace(iso)) return DateTimeOffset.UtcNow;

        return DateTimeOffset.Parse(iso.Trim(), System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal);
    }

    private async static Task<int> HandleScenarioAsync(GlobalOptions g, List<string> argv, CancellationToken cancellationToken)
    {
        if (argv.Count < 1 || argv[0] != "run")
        {
            Console.Error.WriteLine("Usage: scenario run [--prefix PREFIX] [--skip-read-model]");
            return 1;
        }

        argv.RemoveAt(0);
        bool skipReadModel = false;
        for (int i = argv.Count - 1; i >= 0; i--)
            if (string.Equals(argv[i], "--skip-read-model", StringComparison.Ordinal))
            {
                argv.RemoveAt(i);
                skipReadModel = true;
            }

        var parsed = KeyedOptions.Parse(argv, required: Array.Empty<string>());
        string prefix = parsed.Optional("--prefix") ?? "sim";
        parsed.ExpectNoRemainingPositional();

        string deviceIdent = $"{prefix}-device-{Ulid.NewUlid()}";
        string mrn = $"{prefix.ToUpperInvariant()}-MRN-{Ulid.NewUlid().ToString()[..8]}";
        string alertRowKey = $"{prefix}-alert-{Ulid.NewUlid().ToString()[..12]}";

        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(baseUri, g.TenantId, g.BearerToken, g.CorrelationId, g.HttpClientTimeout);
        string v = g.ApiVersion;

        await Console.Error.WriteLineAsync($"Simulation scenario — {baseUri} (API v{v})").ConfigureAwait(false);

        await RunStepAsync(
                "device register",
                GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.Devices(v),
                    new { deviceIdentifier = deviceIdent, manufacturer = "Simulation", initialTrustState = "Active" },
                    cancellationToken,
                    g.TraceHttp),
            cancellationToken)
            .ConfigureAwait(false);

        string sessionId = await CreateSessionAsync(
                client,
                v,
                cancellationToken,
                writeResponseBodyToStdout: false,
                traceHttp: g.TraceHttp)
            .ConfigureAwait(false);

        await RunStepAsync(
                "assign patient",
                GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.SessionPatient(v, sessionId),
                    new { medicalRecordNumber = mrn },
                    cancellationToken,
                    g.TraceHttp),
            cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "link device",
                GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.SessionDevice(v, sessionId),
                    new { deviceIdentifier = deviceIdent },
                    cancellationToken,
                    g.TraceHttp),
            cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "start session",
                GatewayHttp.SendPostWithoutBodyAsync(client, GatewayApiPaths.SessionStart(v, sessionId), g.TraceHttp, cancellationToken),
            cancellationToken)
            .ConfigureAwait(false);

        string payloadJson = """{"demo":true,"map":42}""";
        await RunStepAsync(
                "ingest measurement",
                GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.Measurements(v),
                    new
                    {
                        deviceIdentifier = deviceIdent,
                        channel = "demo",
                        measurementType = "simulation",
                        schemaVersion = "1",
                        rawPayloadJson = payloadJson,
                    },
                    cancellationToken,
                    g.TraceHttp),
            cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "broadcast session feed",
                GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.DeliveryBroadcastSession(v),
                    new
                    {
                        treatmentSessionId = sessionId,
                        eventType = "Simulation.Scenario",
                        summary = "scenario run completed",
                        occurredAtUtc = DateTimeOffset.UtcNow,
                    },
                    cancellationToken,
                    g.TraceHttp),
            cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "broadcast alert feed",
                GatewayHttp.PostJsonAsync(
                    client,
                    GatewayApiPaths.DeliveryBroadcastAlert(v),
                    new
                    {
                        eventType = "Simulation.Scenario",
                        treatmentSessionId = sessionId,
                        alertId = alertRowKey,
                        severity = "High",
                        lifecycleState = "Active",
                        occurredAtUtc = DateTimeOffset.UtcNow,
                    },
                    cancellationToken,
                    g.TraceHttp),
            cancellationToken)
            .ConfigureAwait(false);

        if (!skipReadModel)
        {
            await RunStepAsync(
                    "upsert alert projection",
                    GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.ProjectionsAlerts(v),
                        new
                        {
                            alertRowKey,
                            alertType = "Simulation.Scenario",
                            severity = "High",
                            alertState = "Active",
                            treatmentSessionId = sessionId,
                            raisedAtUtc = DateTimeOffset.UtcNow,
                        },
                        cancellationToken,
                        g.TraceHttp),
                cancellationToken)
                .ConfigureAwait(false);

            await RunStepAsync(
                    "upsert session overview projection",
                    GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.ProjectionsSessionOverview(v),
                        new
                        {
                            treatmentSessionId = sessionId,
                            sessionState = "Active",
                            patientDisplayLabel = mrn,
                            linkedDeviceId = deviceIdent,
                            sessionStartedAtUtc = DateTimeOffset.UtcNow,
                        },
                        cancellationToken,
                        g.TraceHttp),
                cancellationToken)
                .ConfigureAwait(false);
        }
        else await Console.Error.WriteLineAsync("→ skip read-model projection upserts (--skip-read-model).").ConfigureAwait(false);

        var summary = new
        {
            deviceIdentifier = deviceIdent,
            medicalRecordNumber = mrn,
            treatmentSessionId = sessionId,
            alertRowKey,
            readModelProjectionsUpserted = !skipReadModel,
        };
        Console.WriteLine(JsonSerializer.Serialize(summary, GatewayHttp.JsonWriteOptions));
        return 0;
    }

    private async static Task RunStepAsync(string label, Task<HttpResponseMessage> task, CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync($"→ {label}…").ConfigureAwait(false);
        HttpResponseMessage response;
        try
        {
            response = await task.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: {label} — request error: {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await GatewayHttp.WriteResultAsync(response, cancellationToken, writeSuccessBodyToStdout: false)
            .ConfigureAwait(false);
        await Console.Error.WriteLineAsync($"  OK {(int)response.StatusCode}").ConfigureAwait(false);
    }

    private async static Task<string> CreateSessionAsync(
        HttpClient client,
        string v,
        CancellationToken cancellationToken,
        bool writeResponseBodyToStdout = true,
        bool traceHttp = false)
    {
        await Console.Error.WriteLineAsync("→ session create…").ConfigureAwait(false);
        using var msg = new HttpRequestMessage(HttpMethod.Post, GatewayApiPaths.Sessions(v));
        GatewayHttp.TraceHttpRequest(client, msg, traceHttp);
        HttpResponseMessage response = await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            await Console.Error.WriteLineAsync($"HTTP {(int)response.StatusCode}: {body}").ConfigureAwait(false);
            throw new InvalidOperationException("session create failed.");
        }

        CreateSessionDto? dto = JsonSerializer.Deserialize<CreateSessionDto>(body, GatewayHttp.JsonReadOptions);
        if (dto?.SessionId is null || dto.SessionId.Length == 0) throw new InvalidOperationException("session create returned no sessionId.");

        if (writeResponseBodyToStdout) Console.WriteLine(body);
        else await Console.Error.WriteLineAsync($"  OK {(int)response.StatusCode}").ConfigureAwait(false);

        return dto.SessionId;
    }

    private sealed record CreateSessionDto(string SessionId);
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
    /// <summary>Used when SIMULATION_GATEWAY_TIMEOUT_SECONDS is unset and --timeout-seconds not passed.</summary>
    internal const int DefaultHttpClientTimeoutSeconds = 120;

    internal static GlobalOptions Parse(List<string> argv)
    {
        string gateway = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_BASE") ?? "http://localhost:5100";
        string? tenant = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_TENANT");
        string? token = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_BEARER_TOKEN");
        string? correlation = null;
        string apiVersion = "1";
        int timeoutSeconds = DefaultHttpClientTimeoutSeconds;
        bool traceHttp = IsTruthyEnvVar("SIMULATION_GATEWAY_VERBOSE");
        string? timeoutEnv = Environment.GetEnvironmentVariable("SIMULATION_GATEWAY_TIMEOUT_SECONDS");
        if (!string.IsNullOrWhiteSpace(timeoutEnv)
            && int.TryParse(timeoutEnv.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int fromEnv)
            && fromEnv > 0) timeoutSeconds = fromEnv;

        var kept = new List<string>();
        int i = 0;
        while (i < argv.Count)
        {
            string a = argv[i];
            switch (a)
            {
                case "--gateway":
                    gateway = RequirePairValue(argv, ref i, a);
                    continue;
                case "--tenant":
                    tenant = RequirePairValue(argv, ref i, a);
                    continue;
                case "--token":
                    token = RequirePairValue(argv, ref i, a);
                    continue;
                case "--correlation-id":
                    correlation = RequirePairValue(argv, ref i, a);
                    continue;
                case "--api-version":
                    apiVersion = RequirePairValue(argv, ref i, a);
                    continue;
                case "--timeout-seconds":
                    string rawTimeout = RequirePairValue(argv, ref i, a);
                    if (!int.TryParse(rawTimeout.Trim(), System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out int ts) || ts <= 0)
                        throw new ArgumentException("--timeout-seconds must be a positive integer.");

                    timeoutSeconds = ts;
                    continue;
                case "--verbose":
                case "-v":
                    traceHttp = true;
                    i++;
                    continue;
                default:
                    kept.Add(a);
                    i++;
                    break;
            }
        }

        argv.Clear();
        argv.AddRange(kept);
        return new GlobalOptions(
            gateway,
            tenant,
            token,
            correlation,
            apiVersion,
            TimeSpan.FromSeconds(timeoutSeconds),
            traceHttp);
    }

    private static bool IsTruthyEnvVar(string name)
    {
        string? v = Environment.GetEnvironmentVariable(name);
        if (string.IsNullOrWhiteSpace(v)) return false;
        v = v.Trim();
        return string.Equals(v, "1", StringComparison.Ordinal)
            || string.Equals(v, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(v, "yes", StringComparison.OrdinalIgnoreCase);
    }

    private static string RequirePairValue(List<string> argv, ref int index, string optionName)
    {
        if (index + 1 >= argv.Count) throw new ArgumentException($"Missing value for {optionName}.");

        string value = argv[index + 1];
        index += 2;
        return value;
    }
}

internal sealed class KeyedOptions
{
    private readonly Dictionary<string, string> _map = new(StringComparer.Ordinal);
    private readonly List<string> _positional = new();

    internal static KeyedOptions Parse(List<string> argv, string[] required)
    {
        var ko = new KeyedOptions();
        int i = 0;
        while (i < argv.Count)
        {
            string a = argv[i];
            if (a.StartsWith("--", StringComparison.Ordinal))
            {
                if (i + 1 >= argv.Count) throw new ArgumentException($"Missing value for {a}.");

                string key = a;
                string value = argv[i + 1];
                ko._map[key] = value;
                i += 2;
            }
            else
            {
                ko._positional.Add(a);
                i++;
            }
        }

        foreach (string r in required)
            if (!ko._map.ContainsKey(r))
                throw new ArgumentException($"Missing required option {r}.");

        return ko;
    }

    internal string Required(string key) =>
        _map.TryGetValue(key, out string? v) ? v : throw new ArgumentException($"Missing {key}.");

    internal string? Optional(string key) =>
        _map.TryGetValue(key, out string? v) ? v : null;

    internal void ExpectNoRemainingPositional()
    {
        if (_positional.Count > 0) throw new ArgumentException($"Unexpected arguments: {string.Join(' ', _positional)}.");
    }
}
