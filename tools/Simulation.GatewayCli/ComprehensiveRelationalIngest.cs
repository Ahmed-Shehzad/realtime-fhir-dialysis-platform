using System.Text.Json;

namespace Simulation.GatewayCli;

/// <summary>
/// Gateway-only orchestration: admin rule set, session graph, measurements, cross-service writes, reporting, financials, and read-model parity.
/// </summary>
internal static class ComprehensiveRelationalIngest
{
    private const string DefaultRuleVersion = "sim-gateway-1";
    private const string SimulationRulesDocument = /*lang=json,strict*/ "{\"rules\":[{\"id\":\"sim-pass\",\"note\":\"demo rule bundle for publication\"}]}";
    private const string SimulationThresholdPayload = /*lang=json,strict*/ "{\"profile\":\"simulation\",\"mapConcernThresholdMmHg\":65}";
    private const string SimulationProfileCodePrefix = "sim-threshold-";
    private const string WorkflowKindSessionCompletion = "SessionCompletion";
    private const string SurveillanceRuleCodeMapBelow65 = "MAP_BELOW_65";
    private const string AnalyticsModelVersion = "mvp-1";
    private const string ReportNarrativeVersion = "1";
    private const string ProvenanceRelationWasDerivedFrom = "WasDerivedFrom";
    private const string FhirProfileUrlObservationBloodPressure =
        "http://hl7.org/fhir/StructureDefinition/bp";

    private const string WorkflowAdvanceStepFinalizeSession = "FinalizeSession";

    private const string RecoveryPlanCodeSimulation = "sim-recovery";

    private const string ReplayProjectionSetName = "sim-gateway-projections";

    private const string ReplayModeApiDeterministic = "Deterministic";

    public sealed record Summary(
        string DeviceIdentifier,
        string MedicalRecordNumber,
        string TreatmentSessionId,
        string ThresholdProfileId,
        string PublishedRuleSetId,
        string MeasurementIdMap,
        string MeasurementIdSecondary,
        string SurveillanceAlertId,
        string? RuleEvaluationAlertId,
        string WorkflowInstanceId,
        string SessionAnalysisId,
        string SessionReportId,
        string? CoverageRegistrationId,
        string? EligibilityInquiryId,
        string? FinancialClaimId,
        string? PlatformAuditFactIdPrimary,
        string? PlatformAuditFactIdSecondary,
        string? ProvenanceLinkId,
        string DeliveryAlertRowKey,
        bool ReadModelProjectionsUpserted,
        bool FinancialChainExecuted);

    private sealed record BootstrapResult(
        CreateThresholdProfileResponseDto Threshold,
        string RuleSetId,
        string SessionId);

    private sealed record ParallelWaveOutcome(
        RecordPlatformAuditFactResponseDto AuditPrimary,
        string SurveillanceAlertId,
        string? RuleEvaluationAlertId,
        string PrimaryWorkflowId,
        string SessionAnalysisId,
        string ReportId);

    private sealed record AuxiliaryWorkflowStarts(
        WorkflowStartResponseDto Aux2,
        WorkflowStartResponseDto Aux3,
        WorkflowStartResponseDto Aux4,
        WorkflowStartResponseDto Aux5);

    private sealed record FinancialChainIds(string? RegistrationId, string? InquiryId, string? ClaimId);

    private sealed record AuditProvenanceOutcome(string? PrimaryFactId, string? SecondaryFactId, string? LinkId);

    private sealed record SyntheticPublicationDrillContext(
        string Prefix,
        string DeviceIdentifier,
        string SessionId,
        string MidHr);

    private sealed record ParallelWaveContext(
        string DeviceIdentifier,
        string SessionId,
        string MidMap,
        string MidHr,
        string Mrn);

    private sealed record FinancialChainRunContext(
        string Prefix,
        string Mrn,
        string SessionId,
        bool SkipFinancialChain);

    private sealed record ReadModelDeliveryContext(
        string DeviceIdentifier,
        string Mrn,
        string SessionId,
        string AlertRowKey,
        bool SkipReadModelProjections,
        bool SkipProjectionRebuild);

    private sealed record AuditProvenanceFollowUpContext(string SessionId, string Mrn, string AlertRowKey);

    public async static Task<Summary> RunAsync(
        GlobalOptions g,
        string prefix,
        bool skipReadModelProjections,
        bool skipFinancialChain,
        bool skipProjectionRebuild,
        bool skipReplayRecovery,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(g);
        string deviceIdentifier = $"{prefix}-device-{Ulid.NewUlid()}";
        string mrn = $"{prefix.ToUpperInvariant()}-MRN-{Ulid.NewUlid().ToString()[..8]}";
        string alertRowKey = $"{prefix}-alert-{Ulid.NewUlid().ToString()[..12]}";

        Uri baseUri = GatewayHttp.NormalizeGatewayBase(g.GatewayBase);
        using HttpClient client = GatewayHttp.CreateClient(
            baseUri,
            g.TenantId,
            g.BearerToken,
            g.CorrelationId,
            g.HttpClientTimeout);
        string v = g.ApiVersion;

        await Console.Error.WriteLineAsync($"Comprehensive gateway ingest — {baseUri} (API v{v})").ConfigureAwait(false);

        BootstrapResult boot = await RunBootstrapThroughSessionStartAsync(client, v, g, deviceIdentifier, mrn, cancellationToken).ConfigureAwait(false);
        CreateThresholdProfileResponseDto threshold = boot.Threshold;
        string ruleSetId = boot.RuleSetId;
        string sessionId = boot.SessionId;

        (string midMap, string midHr) = await IngestVitalsPairAsync(client, v, g, deviceIdentifier, cancellationToken).ConfigureAwait(false);

        await RunDualMeasurementDerivationAsync(client, v, g, ruleSetId, midMap, midHr, cancellationToken).ConfigureAwait(false);

        await ResolveSessionMeasurementContextsAsync(client, v, g, sessionId, midMap, midHr, cancellationToken).ConfigureAwait(false);

        await RunSyntheticPublicationRetryIngestAndUnresolvedDrillAsync(
                client,
                v,
                g,
                new SyntheticPublicationDrillContext(prefix, deviceIdentifier, sessionId, midHr),
                cancellationToken)
            .ConfigureAwait(false);

        ParallelWaveOutcome wave = await RunCrossServiceParallelWaveAsync(
                client,
                v,
                g,
                new ParallelWaveContext(deviceIdentifier, sessionId, midMap, midHr, mrn),
                cancellationToken)
            .ConfigureAwait(false);

        AuxiliaryWorkflowStarts aux = await StartFourAuxiliaryWorkflowsAsync(client, v, g, sessionId, cancellationToken)
            .ConfigureAwait(false);

        await AdvanceAndCompletePrimaryWorkflowAsync(client, v, g, wave.PrimaryWorkflowId, cancellationToken)
            .ConfigureAwait(false);

        await RunAuxiliaryWorkflowTerminalMutationsAsync(client, v, g, aux, cancellationToken).ConfigureAwait(false);

        await RunSurveillanceAckEscalateResolveAsync(client, v, g, wave.SurveillanceAlertId, cancellationToken)
            .ConfigureAwait(false);

        await RunReportingFinalizeAndPublishAsync(client, v, g, wave.ReportId, cancellationToken).ConfigureAwait(false);

        FinancialChainIds financialIds = await RunFinancialChainIfEnabledAsync(
                client,
                v,
                g,
                new FinancialChainRunContext(prefix, mrn, sessionId, skipFinancialChain),
                cancellationToken)
            .ConfigureAwait(false);

        bool readModelDone = await RunReadModelAndDeliveryIfEnabledAsync(
                client,
                v,
                g,
                new ReadModelDeliveryContext(
                    deviceIdentifier,
                    mrn,
                    sessionId,
                    alertRowKey,
                    skipReadModelProjections,
                    skipProjectionRebuild),
                cancellationToken)
            .ConfigureAwait(false);

        AuditProvenanceOutcome provenance = await RunAuditProvenanceFollowUpIfNeededAsync(
                client,
                v,
                g,
                new AuditProvenanceFollowUpContext(sessionId, mrn, alertRowKey),
                wave.AuditPrimary,
                cancellationToken)
            .ConfigureAwait(false);

        await RunReplayRecoverySequenceIfEnabledAsync(client, v, g, skipReplayRecovery, cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "session: complete",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                            client,
                            GatewayApiPaths.SessionComplete(v, sessionId),
                            g.TraceHttp,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await GatewayHttp.WriteResultAsync(r, cancellationToken, writeSuccessBodyToStdout: false)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        return new Summary(
            deviceIdentifier,
            mrn,
            sessionId,
            threshold.ProfileId,
            ruleSetId,
            midMap,
            midHr,
            wave.SurveillanceAlertId,
            wave.RuleEvaluationAlertId,
            wave.PrimaryWorkflowId,
            wave.SessionAnalysisId,
            wave.ReportId,
            financialIds.RegistrationId,
            financialIds.InquiryId,
            financialIds.ClaimId,
            provenance.PrimaryFactId,
            provenance.SecondaryFactId,
            provenance.LinkId,
            alertRowKey,
            readModelDone,
            !skipFinancialChain);
    }

    /// <summary>
    /// On each tick: ingests vitals, POSTs <c>/delivery/broadcast/session</c> (<c>vitalsByChannel</c>, <c>Simulation.VitalsTrend</c>),
    /// then POSTs <c>/delivery/broadcast/alert</c> so <c>JoinTenantAlerts</c> clients receive <c>alertFeed</c> events.
    /// </summary>
    public async static Task StreamVitalsTrendLoopAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        Summary session,
        TimeSpan tickInterval,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(g);
        ArgumentNullException.ThrowIfNull(session);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(tickInterval, TimeSpan.Zero);

        int tick = 0;
        await Console.Error.WriteLineAsync(
                $"Live stream — vitals ingest + sessionFeed (VitalsTrend, patient hints), tenant alertFeed: "
                + $"every {tickInterval.TotalMilliseconds:0} ms for session {session.TreatmentSessionId} (Ctrl+C to stop).")
            .ConfigureAwait(false);

        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(tickInterval, cancellationToken).ConfigureAwait(false);
            double mapMmHg = 72 + Math.Sin(tick / 4d) * 10 + tick * 0.12 + (tick > 22 ? (tick - 22) * 0.8 : 0);
            double bpm = 68 + Math.Sin(tick / 2.5) * 12 + (tick % 5) * 2;
            double spo2 = 97 + Math.Sin(tick / 6d) * 1.2 - (tick > 26 ? (tick - 26) * 0.15 : 0);
            var sample = new SimulatedVitalsSample(mapMmHg, bpm, spo2);
            await PostVitalsSamplesAndBroadcastAsync(client, v, g, session, sample, tick, cancellationToken)
                .ConfigureAwait(false);
            tick++;
        }
    }

    private const string VitalsTrendEventType = "Simulation.VitalsTrend";

    private readonly record struct SimulatedVitalsSample(double MapMmHg, double Bpm, double Spo2);

    private async static Task PostVitalsSamplesAndBroadcastAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        Summary session,
        SimulatedVitalsSample sample,
        int streamTickIndex,
        CancellationToken cancellationToken)
    {
        const string measurementTypeVitals = "simulation.vitals";
        const string schemaVersion = "1";
        string deviceId = session.DeviceIdentifier;
        string sessionId = session.TreatmentSessionId;
        string payloadMap = JsonSerializer.Serialize(
            new { mapMmHg = Math.Round(sample.MapMmHg, 2), context = "stream" },
            GatewayHttp.JsonWriteOptions);
        string payloadHr = JsonSerializer.Serialize(
            new { bpm = Math.Round(sample.Bpm, 2), context = "stream" },
            GatewayHttp.JsonWriteOptions);
        string payloadSpo2 = JsonSerializer.Serialize(
            new { percent = Math.Round(sample.Spo2, 2), context = "stream" },
            GatewayHttp.JsonWriteOptions);

        _ = await Task.WhenAll(
                GatewayHttp.PostJsonReadAsync<IngestMeasurementResponseDto>(
                    client,
                    GatewayApiPaths.Measurements(v),
                    new
                    {
                        deviceIdentifier = deviceId,
                        channel = "map",
                        measurementType = measurementTypeVitals,
                        schemaVersion,
                        rawPayloadJson = payloadMap,
                    },
                    cancellationToken,
                    g.TraceHttp),
                GatewayHttp.PostJsonReadAsync<IngestMeasurementResponseDto>(
                    client,
                    GatewayApiPaths.Measurements(v),
                    new
                    {
                        deviceIdentifier = deviceId,
                        channel = "heart-rate",
                        measurementType = measurementTypeVitals,
                        schemaVersion,
                        rawPayloadJson = payloadHr,
                    },
                    cancellationToken,
                    g.TraceHttp),
                GatewayHttp.PostJsonReadAsync<IngestMeasurementResponseDto>(
                    client,
                    GatewayApiPaths.Measurements(v),
                    new
                    {
                        deviceIdentifier = deviceId,
                        channel = "spo2",
                        measurementType = measurementTypeVitals,
                        schemaVersion,
                        rawPayloadJson = payloadSpo2,
                    },
                    cancellationToken,
                    g.TraceHttp))
            .ConfigureAwait(false);

        var vitalsByChannel = new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["map"] = sample.MapMmHg,
            ["heart-rate"] = sample.Bpm,
            ["spo2"] = sample.Spo2,
        };

        string sessionStateHint = streamTickIndex % 4 < 2 ? "Active" : "Monitoring";
        string patientDisplayLabel = $"{session.MedicalRecordNumber} · stream {streamTickIndex}";
        HttpResponseMessage broadcast = await GatewayHttp.PostJsonAsync(
                client,
                GatewayApiPaths.DeliveryBroadcastSession(v),
                new
                {
                    treatmentSessionId = sessionId,
                    eventType = VitalsTrendEventType,
                    summary =
                        $"MAP {sample.MapMmHg:0.#} mmHg, HR {sample.Bpm:0.#} /min, SpO₂ {sample.Spo2:0.#} % (simulator stream)",
                    occurredAtUtc = DateTimeOffset.UtcNow,
                    vitalsByChannel,
                    patientDisplayLabel,
                    sessionStateHint,
                    linkedDeviceIdHint = deviceId,
                },
                cancellationToken,
                g.TraceHttp)
            .ConfigureAwait(false);
        await GatewayHttp.ExpectNoContentAsync(broadcast, cancellationToken).ConfigureAwait(false);

        await PostTenantAlertStreamBroadcastAsync(client, v, g, session, sample, streamTickIndex, cancellationToken)
            .ConfigureAwait(false);
    }

    private const string TenantAlertStreamEventVitalsCorrelation = "Simulation.TenantAlert.VitalsCorrelation";
    private const string TenantAlertStreamEventPulse = "Simulation.TenantAlert.StreamPulse";

    private async static Task PostTenantAlertStreamBroadcastAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        Summary session,
        SimulatedVitalsSample sample,
        int streamTickIndex,
        CancellationToken cancellationToken)
    {
        string sessionId = session.TreatmentSessionId;
        string alertId = $"{session.DeliveryAlertRowKey}-stream-{streamTickIndex}";
        bool pulseOnly = streamTickIndex % 3 == 0;
        string eventType = pulseOnly ? TenantAlertStreamEventPulse : TenantAlertStreamEventVitalsCorrelation;
        string severity = sample.MapMmHg >= 82 ? "High" : sample.MapMmHg <= 68 ? "Low" : "Medium";
        string lifecycleState = pulseOnly ? "Active" : streamTickIndex % 2 == 0 ? "Active" : "Acknowledged";
        HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                client,
                GatewayApiPaths.DeliveryBroadcastAlert(v),
                new
                {
                    eventType,
                    treatmentSessionId = sessionId,
                    alertId,
                    severity,
                    lifecycleState,
                    occurredAtUtc = DateTimeOffset.UtcNow,
                },
                cancellationToken,
                g.TraceHttp)
            .ConfigureAwait(false);
        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
    }

    private async static Task<BootstrapResult> RunBootstrapThroughSessionStartAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string deviceIdentifier,
        string mrn,
        CancellationToken cancellationToken)
    {
        string profileCode = $"{SimulationProfileCodePrefix}{Ulid.NewUlid().ToString()[..10]}";
        CreateThresholdProfileResponseDto threshold = await RunJsonStepAsync(
                "admin: create threshold profile",
                GatewayHttp.PostJsonReadAsync<CreateThresholdProfileResponseDto>(
                    client,
                    GatewayApiPaths.AdministrationThresholdProfiles(v),
                    new { profileCode, payloadJson = SimulationThresholdPayload },
                    cancellationToken,
                    g.TraceHttp),
                cancellationToken)
            .ConfigureAwait(false);

        CreateRuleSetDraftResponseDto draft = await RunJsonStepAsync(
                "admin: create rule set draft",
                GatewayHttp.PostJsonReadAsync<CreateRuleSetDraftResponseDto>(
                    client,
                    GatewayApiPaths.AdministrationRuleSets(v),
                    new { ruleVersion = DefaultRuleVersion, rulesDocument = SimulationRulesDocument },
                    cancellationToken,
                    g.TraceHttp),
                cancellationToken)
            .ConfigureAwait(false);

        string ruleSetId = draft.RuleSetId;
        await RunStepAsync(
                "admin: publish rule set",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                            client,
                            GatewayApiPaths.AdministrationRuleSetPublish(v, ruleSetId),
                            g.TraceHttp,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "device register",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.Devices(v),
                            new { deviceIdentifier, manufacturer = "Simulation", initialTrustState = "Active" },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.WriteResultAsync(r, cancellationToken, writeSuccessBodyToStdout: false)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        string sessionId = await CreateTreatmentSessionAsync(client, v, g, cancellationToken).ConfigureAwait(false);

        await RunStepAsync(
                "session assign patient",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.SessionPatient(v, sessionId),
                            new { medicalRecordNumber = mrn },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.WriteResultAsync(r, cancellationToken, writeSuccessBodyToStdout: false)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "session link device",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.SessionDevice(v, sessionId),
                            new { deviceIdentifier },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.WriteResultAsync(r, cancellationToken, writeSuccessBodyToStdout: false)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "session start",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                            client,
                            GatewayApiPaths.SessionStart(v, sessionId),
                            g.TraceHttp,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await GatewayHttp.WriteResultAsync(r, cancellationToken, writeSuccessBodyToStdout: false)
                        .ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        return new BootstrapResult(threshold, ruleSetId, sessionId);
    }

    private async static Task<(string MidMap, string MidHr)> IngestVitalsPairAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string deviceIdentifier,
        CancellationToken cancellationToken)
    {
        const string channelMap = "map";
        const string channelHr = "heart-rate";
        const string measurementTypeVitals = "simulation.vitals";
        const string schemaVersion = "1";
        string payloadMap = """{"mapMmHg":72,"context":"baseline"}""";
        string payloadHr = """{"bpm":68,"context":"same-session"}""";

        await Console.Error.WriteLineAsync("→ parallel: ingest measurements…").ConfigureAwait(false);
        IngestMeasurementResponseDto ingestMap;
        IngestMeasurementResponseDto ingestHr;
        try
        {
            Task<IngestMeasurementResponseDto> tMap = GatewayHttp.PostJsonReadAsync<IngestMeasurementResponseDto>(
                client,
                GatewayApiPaths.Measurements(v),
                new
                {
                    deviceIdentifier,
                    channel = channelMap,
                    measurementType = measurementTypeVitals,
                    schemaVersion,
                    rawPayloadJson = payloadMap,
                },
                cancellationToken,
                g.TraceHttp);
            Task<IngestMeasurementResponseDto> tHr = GatewayHttp.PostJsonReadAsync<IngestMeasurementResponseDto>(
                client,
                GatewayApiPaths.Measurements(v),
                new
                {
                    deviceIdentifier,
                    channel = channelHr,
                    measurementType = measurementTypeVitals,
                    schemaVersion,
                    rawPayloadJson = payloadHr,
                },
                cancellationToken,
                g.TraceHttp);
            _ = await Task.WhenAll(tMap, tHr).ConfigureAwait(false);
            ingestMap = await tMap.ConfigureAwait(false);
            ingestHr = await tHr.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: parallel ingest — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK (2 measurements)").ConfigureAwait(false);
        return (ingestMap.MeasurementId, ingestHr.MeasurementId);
    }

    private async static Task RunDualMeasurementDerivationAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string ruleSetId,
        string midMap,
        string midHr,
        CancellationToken cancellationToken)
    {
        const string channelMap = "map";
        const string channelHr = "heart-rate";
        await Console.Error.WriteLineAsync("→ parallel: per-measurement validation / conditioning / canonical…").ConfigureAwait(false);
        try
        {
            Task<string> tPubMap = RunMeasurementDerivationAsync(
                client,
                v,
                g,
                new MeasurementDerivationWork(midMap, ruleSetId, 72d, channelMap, null),
                cancellationToken);
            Task<string> tPubHr = RunMeasurementDerivationAsync(
                client,
                v,
                g,
                new MeasurementDerivationWork(midHr, ruleSetId, 68d, channelHr, 67d),
                cancellationToken);
            _ = await Task.WhenAll(tPubMap, tPubHr).ConfigureAwait(false);
            _ = await tPubMap.ConfigureAwait(false);
            _ = await tPubHr.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: measurement derivation — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK (2 pipelines)").ConfigureAwait(false);
    }

    private async static Task ResolveSessionMeasurementContextsAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string sessionId,
        string midMap,
        string midHr,
        CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync("→ parallel: session measurement context resolved…").ConfigureAwait(false);
        try
        {
            await Task.WhenAll(
                    PostNoContentAsync(
                        client,
                        GatewayApiPaths.SessionMeasurementContextResolved(v, sessionId, midMap),
                        g.TraceHttp,
                        cancellationToken),
                    PostNoContentAsync(
                        client,
                        GatewayApiPaths.SessionMeasurementContextResolved(v, sessionId, midHr),
                        g.TraceHttp,
                        cancellationToken))
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: context resolved — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK").ConfigureAwait(false);
    }

    private async static Task RunSyntheticPublicationRetryIngestAndUnresolvedDrillAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        SyntheticPublicationDrillContext drill,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(drill);
        string prefix = drill.Prefix;
        string deviceIdentifier = drill.DeviceIdentifier;
        string sessionId = drill.SessionId;
        string midHr = drill.MidHr;
        string measurementIdForStubRetry = $"{prefix}-transient-once-{Ulid.NewUlid()}";
        PublishCanonicalObservationResponseDto stubFailForRetry = await RunJsonStepAsync(
                "clinical: canonical publish (synthetic id, stub transient for retry)",
                GatewayHttp.PostJsonReadAsync<PublishCanonicalObservationResponseDto>(
                    client,
                    GatewayApiPaths.MeasurementCanonicalPublications(v, measurementIdForStubRetry),
                    new { fhirProfileUrl = FhirProfileUrlObservationBloodPressure },
                    cancellationToken,
                    g.TraceHttp),
                cancellationToken)
            .ConfigureAwait(false);
        if (!string.Equals(stubFailForRetry.State, "Failed", StringComparison.Ordinal))
            throw new InvalidOperationException(
                "Expected canonical publication State Failed for stub-transient drill (got "
                + stubFailForRetry.State
                + "). Restart ClinicalInteroperability.Api with current domain (measurement id or profile transient-once).");

        await RunStepAsync(
                "clinical: publication retry (synthetic stub)",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                            client,
                            GatewayApiPaths.PublicationsRetry(v, stubFailForRetry.PublicationId),
                            g.TraceHttp,
                            cancellationToken)
                        .ConfigureAwait(false);
                    string text = await r.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    if (!r.IsSuccessStatusCode)
                    {
                        await Console.Error.WriteLineAsync($"HTTP {(int)r.StatusCode}: {text}").ConfigureAwait(false);
                        throw new InvalidOperationException("publication retry failed.");
                    }

                    _ = JsonSerializer.Deserialize<RetryPublicationResponseDto>(text, GatewayHttp.JsonReadOptions);
                },
                cancellationToken)
            .ConfigureAwait(false);

        const string measurementTypeVitals = "simulation.vitals";
        const string schemaVersion = "1";
        _ = await RunJsonStepAsync(
                "measurement: ingest via /measurements/ingest path",
                GatewayHttp.PostJsonReadAsync<IngestMeasurementResponseDto>(
                    client,
                    GatewayApiPaths.MeasurementsIngest(v),
                    new
                    {
                        deviceIdentifier,
                        channel = "ingest-path",
                        measurementType = measurementTypeVitals,
                        schemaVersion,
                        rawPayloadJson = """{"note":"explicit /ingest route"}""",
                    },
                    cancellationToken,
                    g.TraceHttp),
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "session: measurement context unresolved (HR drill)",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.SessionMeasurementContextUnresolved(v, sessionId, midHr),
                            new { reason = "Simulation.GatewayCli POST coverage drill after resolved." },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async static Task<ParallelWaveOutcome> RunCrossServiceParallelWaveAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        ParallelWaveContext waveContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(waveContext);
        string deviceIdentifier = waveContext.DeviceIdentifier;
        string sessionId = waveContext.SessionId;
        string midMap = waveContext.MidMap;
        string midHr = waveContext.MidHr;
        string mrn = waveContext.Mrn;
        string auditDetailPrimary = JsonSerializer.Serialize(
            new
            {
                treatmentSessionId = sessionId,
                deviceIdentifier,
                measurementIds = new[] { midMap, midHr },
            },
            GatewayHttp.JsonWriteOptions);

        await Console.Error.WriteLineAsync("→ parallel: audit / surveillance / terminology / rule / workflow / analytics / reporting.generate…").ConfigureAwait(false);
        RecordPlatformAuditFactResponseDto? auditPrimary;
        RaiseAlertResponseDto? surveillanceRaise;
        WorkflowStartResponseDto? workflow;
        RunSessionAnalysisResponseDto? analysis;
        GenerateSessionReportResponseDto? report;
        EvaluateRuleResponseDto? ruleEval;
        try
        {
            Task<RecordPlatformAuditFactResponseDto> tAudit = GatewayHttp.PostJsonReadAsync<RecordPlatformAuditFactResponseDto>(
                client,
                GatewayApiPaths.AuditFacts(v),
                new
                {
                    occurredAtUtc = DateTimeOffset.UtcNow,
                    eventType = "Simulation.RelationalIngest",
                    summary = "Initial audit fact for simulated session graph",
                    detailJson = auditDetailPrimary,
                    correlationId = g.CorrelationId,
                    causationId = (string?)null,
                    actorId = (string?)null,
                    sourceSystem = "Simulation.GatewayCli",
                    relatedResourceType = "TreatmentSession",
                    relatedResourceId = sessionId,
                    sessionId,
                    patientId = mrn,
                },
                cancellationToken,
                g.TraceHttp);
            Task<RaiseAlertResponseDto> tSurv = GatewayHttp.PostJsonReadAsync<RaiseAlertResponseDto>(
                client,
                GatewayApiPaths.SurveillanceAlerts(v),
                new
                {
                    treatmentSessionId = sessionId,
                    alertType = "Simulation.ClinicalSignal",
                    severity = "Medium",
                    detail = "Simulated surveillance alert tied to ingest run (parallel wave).",
                },
                cancellationToken,
                g.TraceHttp);
            Task<ValidateSemanticConformanceResponseDto> tTermMap = GatewayHttp.PostJsonReadAsync<ValidateSemanticConformanceResponseDto>(
                client,
                GatewayApiPaths.ResourceSemanticConformance(v, midMap),
                new
                {
                    codeSystemUri = "http://loinc.org",
                    codeValue = "85354-9",
                    unitCode = "mm[Hg]",
                    profileUrl = FhirProfileUrlObservationBloodPressure,
                },
                cancellationToken,
                g.TraceHttp);
            Task<ValidateSemanticConformanceResponseDto> tTermHr = GatewayHttp.PostJsonReadAsync<ValidateSemanticConformanceResponseDto>(
                client,
                GatewayApiPaths.ResourceSemanticConformance(v, midHr),
                new
                {
                    codeSystemUri = "http://loinc.org",
                    codeValue = "8867-4",
                    unitCode = "/min",
                    profileUrl = (string?)null,
                },
                cancellationToken,
                g.TraceHttp);
            Task<EvaluateRuleResponseDto> tRule = GatewayHttp.PostJsonReadAsync<EvaluateRuleResponseDto>(
                client,
                GatewayApiPaths.SurveillanceRulesEvaluate(v),
                new
                {
                    treatmentSessionId = sessionId,
                    ruleCode = SurveillanceRuleCodeMapBelow65,
                    metricValueMmHg = 60d,
                },
                cancellationToken,
                g.TraceHttp);
            Task<WorkflowStartResponseDto> tWf = GatewayHttp.PostJsonReadAsync<WorkflowStartResponseDto>(
                client,
                GatewayApiPaths.WorkflowStart(v),
                new { workflowKind = WorkflowKindSessionCompletion, treatmentSessionId = sessionId },
                cancellationToken,
                g.TraceHttp);
            Task<RunSessionAnalysisResponseDto> tAnalysis = GatewayHttp.PostJsonReadAsync<RunSessionAnalysisResponseDto>(
                client,
                GatewayApiPaths.ClinicalAnalyticsSessionAnalyses(v, sessionId),
                new { modelVersion = AnalyticsModelVersion },
                cancellationToken,
                g.TraceHttp);

            await Task.WhenAll(
                    tAudit,
                    tSurv,
                    tTermMap,
                    tTermHr,
                    tRule,
                    tWf,
                    tAnalysis)
                .ConfigureAwait(false);

            auditPrimary = await tAudit.ConfigureAwait(false);
            surveillanceRaise = await tSurv.ConfigureAwait(false);
            _ = await tTermMap.ConfigureAwait(false);
            _ = await tTermHr.ConfigureAwait(false);
            ruleEval = await tRule.ConfigureAwait(false);
            workflow = await tWf.ConfigureAwait(false);
            analysis = await tAnalysis.ConfigureAwait(false);

            object reportBody = new
            {
                narrativeVersion = ReportNarrativeVersion,
                evidence = new object[]
                {
                    new { kind = "CanonicalObservation", locator = midMap },
                    new { kind = "CanonicalObservation", locator = midHr },
                    new { kind = "SessionAnalysis", locator = analysis.AnalysisId },
                    new { kind = "Other", locator = sessionId },
                },
            };
            report = await GatewayHttp
                .PostJsonReadAsync<GenerateSessionReportResponseDto>(
                    client,
                    GatewayApiPaths.ReportingSessionReports(v, sessionId),
                    reportBody,
                    cancellationToken,
                    g.TraceHttp)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: cross-service parallel wave — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK").ConfigureAwait(false);
        return new ParallelWaveOutcome(
            auditPrimary!,
            surveillanceRaise!.AlertId,
            ruleEval!.AlertId,
            workflow!.WorkflowInstanceId,
            analysis!.AnalysisId,
            report!.ReportId);
    }

    private async static Task<AuxiliaryWorkflowStarts> StartFourAuxiliaryWorkflowsAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string sessionId,
        CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync("→ parallel: auxiliary workflow starts (SessionCompletion)…").ConfigureAwait(false);
        WorkflowStartResponseDto auxWf2;
        WorkflowStartResponseDto auxWf3;
        WorkflowStartResponseDto auxWf4;
        WorkflowStartResponseDto auxWf5;
        try
        {
            Task<WorkflowStartResponseDto> t2 = GatewayHttp.PostJsonReadAsync<WorkflowStartResponseDto>(
                client,
                GatewayApiPaths.WorkflowStart(v),
                new { workflowKind = WorkflowKindSessionCompletion, treatmentSessionId = sessionId },
                cancellationToken,
                g.TraceHttp);
            Task<WorkflowStartResponseDto> t3 = GatewayHttp.PostJsonReadAsync<WorkflowStartResponseDto>(
                client,
                GatewayApiPaths.WorkflowStart(v),
                new { workflowKind = WorkflowKindSessionCompletion, treatmentSessionId = sessionId },
                cancellationToken,
                g.TraceHttp);
            Task<WorkflowStartResponseDto> t4 = GatewayHttp.PostJsonReadAsync<WorkflowStartResponseDto>(
                client,
                GatewayApiPaths.WorkflowStart(v),
                new { workflowKind = WorkflowKindSessionCompletion, treatmentSessionId = sessionId },
                cancellationToken,
                g.TraceHttp);
            Task<WorkflowStartResponseDto> t5 = GatewayHttp.PostJsonReadAsync<WorkflowStartResponseDto>(
                client,
                GatewayApiPaths.WorkflowStart(v),
                new { workflowKind = WorkflowKindSessionCompletion, treatmentSessionId = sessionId },
                cancellationToken,
                g.TraceHttp);
            _ = await Task.WhenAll(t2, t3, t4, t5).ConfigureAwait(false);
            auxWf2 = await t2.ConfigureAwait(false);
            auxWf3 = await t3.ConfigureAwait(false);
            auxWf4 = await t4.ConfigureAwait(false);
            auxWf5 = await t5.ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: auxiliary workflow starts — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK (4 starts)").ConfigureAwait(false);
        return new AuxiliaryWorkflowStarts(auxWf2, auxWf3, auxWf4, auxWf5);
    }

    private async static Task AdvanceAndCompletePrimaryWorkflowAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string primaryWorkflowId,
        CancellationToken cancellationToken)
    {
        await RunStepAsync(
                "workflow: advance primary",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.WorkflowAdvance(v, primaryWorkflowId),
                            new { nextStepName = WorkflowAdvanceStepFinalizeSession },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "workflow: complete primary",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                            client,
                            GatewayApiPaths.WorkflowComplete(v, primaryWorkflowId),
                            g.TraceHttp,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async static Task RunAuxiliaryWorkflowTerminalMutationsAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        AuxiliaryWorkflowStarts aux,
        CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync("→ parallel: auxiliary workflow terminal mutations…").ConfigureAwait(false);
        try
        {
            async Task FailAuxAsync()
            {
                HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.WorkflowFail(v, aux.Aux2.WorkflowInstanceId),
                        new { reason = "Simulation.GatewayCli coverage (fail branch)." },
                        cancellationToken,
                        g.TraceHttp)
                    .ConfigureAwait(false);
                await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
            }

            async Task CompensateAuxAsync()
            {
                HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.WorkflowCompensation(v, aux.Aux3.WorkflowInstanceId),
                        new { reason = "Simulation.GatewayCli coverage (compensation)." },
                        cancellationToken,
                        g.TraceHttp)
                    .ConfigureAwait(false);
                await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
            }

            async Task ManualAuxAsync()
            {
                HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.WorkflowManualIntervention(v, aux.Aux4.WorkflowInstanceId),
                        new { detail = "Simulation.GatewayCli coverage (manual intervention)." },
                        cancellationToken,
                        g.TraceHttp)
                    .ConfigureAwait(false);
                await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
            }

            async Task TimeoutAuxAsync()
            {
                HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                        client,
                        GatewayApiPaths.WorkflowTimeout(v, aux.Aux5.WorkflowInstanceId),
                        g.TraceHttp,
                        cancellationToken)
                    .ConfigureAwait(false);
                await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
            }

            await Task.WhenAll(FailAuxAsync(), CompensateAuxAsync(), ManualAuxAsync(), TimeoutAuxAsync()).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: auxiliary workflow terminals — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK").ConfigureAwait(false);
    }

    private async static Task RunSurveillanceAckEscalateResolveAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string survAlertId,
        CancellationToken cancellationToken)
    {
        await RunStepAsync(
                "surveillance: acknowledge",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.SurveillanceAlertAcknowledge(v, survAlertId),
                            new { acknowledgedByUserId = "simulator-user" },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "surveillance: escalate",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.SurveillanceAlertEscalate(v, survAlertId),
                            new { escalationDetail = "Simulation.GatewayCli coverage escalation." },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "surveillance: resolve",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.SurveillanceAlertResolve(v, survAlertId),
                            new { resolutionNote = "Simulation.GatewayCli coverage resolve." },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async static Task RunReportingFinalizeAndPublishAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        string reportId,
        CancellationToken cancellationToken)
    {
        await RunStepAsync(
                "reporting: finalize",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                            client,
                            GatewayApiPaths.ReportingFinalizeReport(v, reportId),
                            g.TraceHttp,
                            cancellationToken)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);

        await RunStepAsync(
                "reporting: publish",
                async () =>
                {
                    HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                            client,
                            GatewayApiPaths.ReportingPublishReport(v, reportId),
                            new { publicationTargetHint = "simulation-ehr" },
                            cancellationToken,
                            g.TraceHttp)
                        .ConfigureAwait(false);
                    await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                },
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async static Task<FinancialChainIds> RunFinancialChainIfEnabledAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        FinancialChainRunContext chainContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(chainContext);
        string prefix = chainContext.Prefix;
        string mrn = chainContext.Mrn;
        string sessionId = chainContext.SessionId;
        bool skipFinancialChain = chainContext.SkipFinancialChain;
        string? registrationId = null;
        string? inquiryId = null;
        string? claimId = null;
        if (!skipFinancialChain)
        {
            DateOnly periodStart = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
            DateOnly? periodEnd = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(180));
            RecordCoverageRegistrationResponseDto coverage = await RunJsonStepAsync(
                    "financial: coverage registration",
                    GatewayHttp.PostJsonReadAsync<RecordCoverageRegistrationResponseDto>(
                        client,
                        GatewayApiPaths.FinancialCoverageRegistrations(v),
                        new
                        {
                            patientId = mrn,
                            memberIdentifier = $"{prefix}-member-{Ulid.NewUlid().ToString()[..8]}",
                            payorDisplayName = "Simulation Payor",
                            planDisplayName = "Simulation Plan",
                            periodStart,
                            periodEnd,
                            fhirCoverageResourceId = $"Coverage/{prefix}-cov",
                        },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);
            registrationId = coverage.RegistrationId;

            RecordEligibilityResponseDto eligibility = await RunJsonStepAsync(
                    "financial: eligibility",
                    GatewayHttp.PostJsonReadAsync<RecordEligibilityResponseDto>(
                        client,
                        GatewayApiPaths.FinancialCoverageEligibility(v),
                        new
                        {
                            patientCoverageRegistrationId = registrationId,
                            patientId = mrn,
                            outcomeCode = "ActiveCoverage",
                            notes = "Simulation eligibility check (gateway ingest).",
                        },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);
            inquiryId = eligibility.InquiryId;

            SubmitClaimResponseDto claim = await RunJsonStepAsync(
                    "financial: submit claim",
                    GatewayHttp.PostJsonReadAsync<SubmitClaimResponseDto>(
                        client,
                        GatewayApiPaths.FinancialClaims(v),
                        new
                        {
                            treatmentSessionId = sessionId,
                            patientId = mrn,
                            patientCoverageRegistrationId = registrationId,
                            fhirEncounterReference = $"Encounter/{prefix}-enc",
                            claimUse = "Normal",
                            externalClaimId = $"{prefix}-claim-ext-{Ulid.NewUlid().ToString()[..10]}",
                        },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);
            claimId = claim.FinancialClaimId;

            _ = await RunJsonStepAsync(
                    "financial: adjudication",
                    GatewayHttp.PostJsonReadAsync<RecordAdjudicationResponseDto>(
                        client,
                        GatewayApiPaths.FinancialClaimAdjudication(v, claimId),
                        new
                        {
                            externalClaimResponseId = $"{prefix}-835-{Ulid.NewUlid().ToString()[..8]}",
                            outcomeDisplay = "Paid",
                        },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);

            _ = await RunJsonStepAsync(
                    "financial: explanation of benefit",
                    GatewayHttp.PostJsonReadAsync<AttachEobResponseDto>(
                        client,
                        GatewayApiPaths.FinancialExplanationOfBenefit(v),
                        new
                        {
                            dialysisFinancialClaimId = claimId,
                            treatmentSessionId = sessionId,
                            fhirExplanationOfBenefitReference = $"ExplanationOfBenefit/{prefix}-eob",
                            patientResponsibilityAmount = 25.50m,
                        },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);
        }
        else await Console.Error.WriteLineAsync("→ skip financial chain (--skip-financial).").ConfigureAwait(false);

        return new FinancialChainIds(registrationId, inquiryId, claimId);
    }

    private async static Task<bool> RunReadModelAndDeliveryIfEnabledAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        ReadModelDeliveryContext deliveryContext,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(deliveryContext);
        if (deliveryContext.SkipReadModelProjections)
        {
            await Console.Error.WriteLineAsync("→ skip read-model projection upserts (--skip-read-model).").ConfigureAwait(false);
            return false;
        }

        await RebuildProjectionsIfRequestedAsync(
                client,
                v,
                g,
                deliveryContext.SkipProjectionRebuild,
                cancellationToken)
            .ConfigureAwait(false);

        await PostReadModelDeliveryAndProjectionUpsertsAsync(client, v, g, deliveryContext, cancellationToken)
            .ConfigureAwait(false);

        return true;
    }

    private async static Task RebuildProjectionsIfRequestedAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        bool skipProjectionRebuild,
        CancellationToken cancellationToken)
    {
        if (skipProjectionRebuild)
        {
            await Console.Error.WriteLineAsync("→ skip projections/rebuild (--skip-projection-rebuild).").ConfigureAwait(false);
            return;
        }

        await RunStepAsync(
                "read-model: rebuild projections",
                async () => await ExecuteSingleProjectionRebuildAsync(client, v, g, cancellationToken).ConfigureAwait(false),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private async static Task ExecuteSingleProjectionRebuildAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                client,
                GatewayApiPaths.ProjectionsRebuild(v),
                g.TraceHttp,
                cancellationToken)
            .ConfigureAwait(false);
        string text = await r.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!r.IsSuccessStatusCode)
        {
            await Console.Error.WriteLineAsync($"HTTP {(int)r.StatusCode}: {text}").ConfigureAwait(false);
            throw new InvalidOperationException("projections rebuild failed.");
        }

        _ = JsonSerializer.Deserialize<RebuildProjectionsResponseDto>(text, GatewayHttp.JsonReadOptions);
    }

    private async static Task PostReadModelDeliveryAndProjectionUpsertsAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        ReadModelDeliveryContext d,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(d);
        string sessionId = d.SessionId;
        string alertRowKey = d.AlertRowKey;
        string mrn = d.Mrn;
        string deviceIdentifier = d.DeviceIdentifier;

        await Console.Error.WriteLineAsync("→ parallel: delivery broadcasts + read-model projections…").ConfigureAwait(false);
        try
        {
            HttpResponseMessage[] responses = await Task.WhenAll(
                    GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.DeliveryBroadcastSession(v),
                        new
                        {
                            treatmentSessionId = sessionId,
                            eventType = "Simulation.RelationalIngest",
                            summary = "Session feed: comprehensive ingest completed",
                            occurredAtUtc = DateTimeOffset.UtcNow,
                        },
                        cancellationToken,
                        g.TraceHttp),
                    GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.DeliveryBroadcastAlert(v),
                        new
                        {
                            eventType = "Simulation.RelationalIngest",
                            treatmentSessionId = sessionId,
                            alertId = alertRowKey,
                            severity = "High",
                            lifecycleState = "Active",
                            occurredAtUtc = DateTimeOffset.UtcNow,
                        },
                        cancellationToken,
                        g.TraceHttp),
                    GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.ProjectionsAlerts(v),
                        new
                        {
                            alertRowKey,
                            alertType = "Simulation.RelationalIngest",
                            severity = "High",
                            alertState = "Active",
                            treatmentSessionId = sessionId,
                            raisedAtUtc = DateTimeOffset.UtcNow,
                        },
                        cancellationToken,
                        g.TraceHttp),
                    GatewayHttp.PostJsonAsync(
                        client,
                        GatewayApiPaths.ProjectionsSessionOverview(v),
                        new
                        {
                            treatmentSessionId = sessionId,
                            sessionState = "Active",
                            patientDisplayLabel = mrn,
                            linkedDeviceId = deviceIdentifier,
                            sessionStartedAtUtc = DateTimeOffset.UtcNow,
                        },
                        cancellationToken,
                        g.TraceHttp))
                .ConfigureAwait(false);

            foreach (HttpResponseMessage r in responses)
                await GatewayHttp.WriteResultAsync(r, cancellationToken, writeSuccessBodyToStdout: false)
                    .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: delivery / projections — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK").ConfigureAwait(false);
    }

    private async static Task<AuditProvenanceOutcome> RunAuditProvenanceFollowUpIfNeededAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        AuditProvenanceFollowUpContext followUpContext,
        RecordPlatformAuditFactResponseDto auditPrimary,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(followUpContext);
        string sessionId = followUpContext.SessionId;
        string mrn = followUpContext.Mrn;
        string alertRowKey = followUpContext.AlertRowKey;
        string? factPrimary = auditPrimary.PlatformAuditFactId;
        string? factSecondaryId = null;
        string? linkId = null;
        switch (factPrimary)
        {
            case { Length: > 0 }:
            {
                string auditDetailSecondary = JsonSerializer.Serialize(
                    new { treatmentSessionId = sessionId, phase = "post-read-model", alertRowKey },
                    GatewayHttp.JsonWriteOptions);
                RecordPlatformAuditFactResponseDto secondary = await RunJsonStepAsync(
                        "audit: second fact (provenance anchor)",
                        GatewayHttp.PostJsonReadAsync<RecordPlatformAuditFactResponseDto>(
                            client,
                            GatewayApiPaths.AuditFacts(v),
                            new
                            {
                                occurredAtUtc = DateTimeOffset.UtcNow,
                                eventType = "Simulation.RelationalIngest.FollowUp",
                                summary = "Second audit fact linked from the first for provenance drill-down",
                                detailJson = auditDetailSecondary,
                                correlationId = g.CorrelationId,
                                causationId = factPrimary,
                                actorId = (string?)null,
                                sourceSystem = "Simulation.GatewayCli",
                                relatedResourceType = "TreatmentSession",
                                relatedResourceId = sessionId,
                                sessionId,
                                patientId = mrn,
                            },
                            cancellationToken,
                            g.TraceHttp),
                        cancellationToken)
                    .ConfigureAwait(false);
                factSecondaryId = secondary.PlatformAuditFactId;

                RecordProvenanceLinkResponseDto link = await RunJsonStepAsync(
                        "audit: provenance link",
                        GatewayHttp.PostJsonReadAsync<RecordProvenanceLinkResponseDto>(
                            client,
                            GatewayApiPaths.AuditProvenanceLinks(v),
                            new
                            {
                                fromPlatformAuditFactId = factSecondaryId,
                                toPlatformAuditFactId = factPrimary,
                                relationType = ProvenanceRelationWasDerivedFrom,
                            },
                            cancellationToken,
                            g.TraceHttp),
                        cancellationToken)
                    .ConfigureAwait(false);
                linkId = link.ProvenanceLinkId;
                break;
            }
        }

        return new AuditProvenanceOutcome(factPrimary, factSecondaryId, linkId);
    }

    private async static Task RunReplayRecoverySequenceIfEnabledAsync(
        HttpClient client,
        string v,
        GlobalOptions g,
        bool skipReplayRecovery,
        CancellationToken cancellationToken)
    {
        if (!skipReplayRecovery)
        {
            _ = await RunJsonStepAsync(
                    "replay-recovery: execute recovery plan",
                    GatewayHttp.PostJsonReadAsync<ExecuteRecoveryPlanResponseDto>(
                        client,
                        GatewayApiPaths.ReplayRecoveryRecoveryPlansExecute(v),
                        new { planCode = RecoveryPlanCodeSimulation },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);

            StartReplayJobResponseDto replayJobMain = await RunJsonStepAsync(
                    "replay-recovery: replay job start (deterministic)",
                    GatewayHttp.PostJsonReadAsync<StartReplayJobResponseDto>(
                        client,
                        GatewayApiPaths.ReplayRecoveryReplayJobsStart(v),
                        new { replayMode = ReplayModeApiDeterministic, projectionSetName = ReplayProjectionSetName },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);

            string replayJobIdMain = replayJobMain.ReplayJobId;
            await RunStepAsync(
                    "replay-recovery: advance checkpoint",
                    async () =>
                    {
                        HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                                client,
                                GatewayApiPaths.ReplayRecoveryReplayJobCheckpoints(v, replayJobIdMain),
                                g.TraceHttp,
                                cancellationToken)
                            .ConfigureAwait(false);
                        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            await RunStepAsync(
                    "replay-recovery: pause",
                    async () =>
                    {
                        HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                                client,
                                GatewayApiPaths.ReplayRecoveryReplayJobPause(v, replayJobIdMain),
                                g.TraceHttp,
                                cancellationToken)
                            .ConfigureAwait(false);
                        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            await RunStepAsync(
                    "replay-recovery: resume",
                    async () =>
                    {
                        HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                                client,
                                GatewayApiPaths.ReplayRecoveryReplayJobResume(v, replayJobIdMain),
                                g.TraceHttp,
                                cancellationToken)
                            .ConfigureAwait(false);
                        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            await RunStepAsync(
                    "replay-recovery: complete job",
                    async () =>
                    {
                        HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(
                                client,
                                GatewayApiPaths.ReplayRecoveryReplayJobComplete(v, replayJobIdMain),
                                g.TraceHttp,
                                cancellationToken)
                            .ConfigureAwait(false);
                        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                    },
                    cancellationToken)
                .ConfigureAwait(false);

            StartReplayJobResponseDto replayJobFail = await RunJsonStepAsync(
                    "replay-recovery: second job start (for fail endpoint)",
                    GatewayHttp.PostJsonReadAsync<StartReplayJobResponseDto>(
                        client,
                        GatewayApiPaths.ReplayRecoveryReplayJobsStart(v),
                        new { replayMode = ReplayModeApiDeterministic, projectionSetName = $"{ReplayProjectionSetName}-fail" },
                        cancellationToken,
                        g.TraceHttp),
                    cancellationToken)
                .ConfigureAwait(false);

            await RunStepAsync(
                    "replay-recovery: fail second job",
                    async () =>
                    {
                        HttpResponseMessage r = await GatewayHttp.PostJsonAsync(
                                client,
                                GatewayApiPaths.ReplayRecoveryReplayJobFail(v, replayJobFail.ReplayJobId),
                                new { reason = "Simulation.GatewayCli coverage (replay job fail)." },
                                cancellationToken,
                                g.TraceHttp)
                            .ConfigureAwait(false);
                        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        else await Console.Error.WriteLineAsync("→ skip replay-recovery (--skip-replay-recovery).").ConfigureAwait(false);
    }

    private readonly record struct MeasurementDerivationWork(
        string MeasurementId,
        string ValidationProfileId,
        double SampleValue,
        string ChannelId,
        double? PreviousSampleValue);

    private async static Task<string> RunMeasurementDerivationAsync(
        HttpClient client,
        string apiVersion,
        GlobalOptions g,
        MeasurementDerivationWork work,
        CancellationToken cancellationToken)
    {
        _ = await GatewayHttp.PostJsonReadAsync<ValidateMeasurementResponseDto>(
                client,
                GatewayApiPaths.MeasurementValidation(apiVersion, work.MeasurementId),
                new { validationProfileId = work.ValidationProfileId, sampleValue = work.SampleValue },
                cancellationToken,
                g.TraceHttp)
            .ConfigureAwait(false);
        _ = await GatewayHttp.PostJsonReadAsync<ConditionSignalResponseDto>(
                client,
                GatewayApiPaths.MeasurementConditioning(apiVersion, work.MeasurementId),
                new
                {
                    channelId = work.ChannelId,
                    sampleValue = work.SampleValue,
                    previousSampleValue = work.PreviousSampleValue,
                },
                cancellationToken,
                g.TraceHttp)
            .ConfigureAwait(false);
        PublishCanonicalObservationResponseDto pub = await GatewayHttp.PostJsonReadAsync<PublishCanonicalObservationResponseDto>(
                client,
                GatewayApiPaths.MeasurementCanonicalPublications(apiVersion, work.MeasurementId),
                new { fhirProfileUrl = FhirProfileUrlObservationBloodPressure },
                cancellationToken,
                g.TraceHttp)
            .ConfigureAwait(false);
        return pub.PublicationId;
    }

    private async static Task PostNoContentAsync(
        HttpClient client,
        string relativePath,
        bool traceHttp,
        CancellationToken cancellationToken)
    {
        HttpResponseMessage r = await GatewayHttp.SendPostWithoutBodyAsync(client, relativePath, traceHttp, cancellationToken)
            .ConfigureAwait(false);
        await GatewayHttp.ExpectNoContentAsync(r, cancellationToken).ConfigureAwait(false);
    }

    private async static Task<string> CreateTreatmentSessionAsync(
        HttpClient client,
        string apiVersion,
        GlobalOptions g,
        CancellationToken cancellationToken)
    {
        await Console.Error.WriteLineAsync("→ session create…").ConfigureAwait(false);
        using var msg = new HttpRequestMessage(HttpMethod.Post, GatewayApiPaths.Sessions(apiVersion));
        GatewayHttp.TraceHttpRequest(client, msg, g.TraceHttp);
        HttpResponseMessage response = await client.SendAsync(msg, cancellationToken).ConfigureAwait(false);
        string body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            await Console.Error.WriteLineAsync($"HTTP {(int)response.StatusCode}: {body}").ConfigureAwait(false);
            throw new InvalidOperationException("session create failed.");
        }

        CreateSessionResponseDto? dto = JsonSerializer.Deserialize<CreateSessionResponseDto>(body, GatewayHttp.JsonReadOptions);
        if (dto?.SessionId is null || dto.SessionId.Length == 0)
            throw new InvalidOperationException("session create returned no sessionId.");

        await Console.Error.WriteLineAsync($"  OK {(int)response.StatusCode}").ConfigureAwait(false);
        return dto.SessionId;
    }

    private async static Task RunStepAsync(string label, Func<Task> action, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Console.Error.WriteLineAsync($"→ {label}…").ConfigureAwait(false);
        try
        {
            await action().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: {label} — {ex.Message}").ConfigureAwait(false);
            throw;
        }

        await Console.Error.WriteLineAsync("  OK").ConfigureAwait(false);
    }

    private async static Task<T> RunJsonStepAsync<T>(string label, Task<T> task, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Console.Error.WriteLineAsync($"→ {label}…").ConfigureAwait(false);
        try
        {
            T result = await task.ConfigureAwait(false);
            await Console.Error.WriteLineAsync("  OK").ConfigureAwait(false);
            return result;
        }
        catch (Exception ex)
        {
            await Console.Error.WriteLineAsync($"Step failed: {label} — {ex.Message}").ConfigureAwait(false);
            throw;
        }
    }

    private sealed record CreateSessionResponseDto(string SessionId);

    private sealed record CreateThresholdProfileResponseDto(string ProfileId);

    private sealed record CreateRuleSetDraftResponseDto(string RuleSetId);

    private sealed record IngestMeasurementResponseDto(string MeasurementId);

    private sealed record ValidateMeasurementResponseDto(string ValidationId, string Outcome);

    private sealed record ConditionSignalResponseDto(string ConditioningResultId, bool IsDropout, int QualityScorePercent);

    private sealed record PublishCanonicalObservationResponseDto(
        string PublicationId,
        string State,
        string ObservationId,
        string? FhirResourceReference);

    private sealed record RecordPlatformAuditFactResponseDto(string PlatformAuditFactId);

    private sealed record RecordProvenanceLinkResponseDto(string ProvenanceLinkId);

    private sealed record RaiseAlertResponseDto(string AlertId);

    private sealed record ValidateSemanticConformanceResponseDto(
        string AssessmentId,
        string TerminologyOutcome,
        string ProfileOutcome);

    private sealed record EvaluateRuleResponseDto(bool AlertRaised, string? AlertId);

    private sealed record WorkflowStartResponseDto(string WorkflowInstanceId);

    private sealed record RunSessionAnalysisResponseDto(string AnalysisId);

    private sealed record GenerateSessionReportResponseDto(string ReportId);

    private sealed record RecordCoverageRegistrationResponseDto(string RegistrationId);

    private sealed record RecordEligibilityResponseDto(string InquiryId);

    private sealed record SubmitClaimResponseDto(string FinancialClaimId);

    private sealed record RecordAdjudicationResponseDto(bool Updated);

    private sealed record AttachEobResponseDto(string ExplanationOfBenefitRecordId, bool Created);

    private sealed record RetryPublicationResponseDto(
        string State,
        string ObservationId,
        string? FhirResourceReference);

    private sealed record RebuildProjectionsResponseDto(int ProjectionRowsSeeded);

    private sealed record ExecuteRecoveryPlanResponseDto(string ExecutionId);

    private sealed record StartReplayJobResponseDto(string ReplayJobId);
}
