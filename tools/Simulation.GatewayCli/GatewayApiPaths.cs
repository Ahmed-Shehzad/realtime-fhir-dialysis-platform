namespace Simulation.GatewayCli;

/// <summary>
/// Relative URLs under the gateway base (see RealtimePlatform.ApiGateway ReverseProxy routes and service controllers).
/// </summary>
internal static class GatewayApiPaths
{
    internal static string Devices(string apiVersion) => $"api/v{apiVersion}/devices";

    /// <summary>GET devices/{deviceId}/trust (DeviceRegistry.Api).</summary>
    internal static string DeviceTrust(string apiVersion, string deviceId) =>
        $"{Devices(apiVersion)}/{Uri.EscapeDataString(deviceId)}/trust";

    internal static string Sessions(string apiVersion) => $"api/v{apiVersion}/sessions";

    internal static string SessionPatient(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/patient";

    internal static string SessionDevice(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/device";

    internal static string SessionStart(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/start";

    internal static string SessionComplete(string apiVersion, string sessionId) =>
        $"{Sessions(apiVersion)}/{Uri.EscapeDataString(sessionId)}/complete";

    internal static string Measurements(string apiVersion) => $"api/v{apiVersion}/measurements";

    internal static string DeliveryBroadcastSession(string apiVersion) =>
        $"api/v{apiVersion}/delivery/broadcast/session";

    internal static string DeliveryBroadcastAlert(string apiVersion) =>
        $"api/v{apiVersion}/delivery/broadcast/alert";

    internal static string ProjectionsAlerts(string apiVersion) => $"api/v{apiVersion}/projections/alerts";

    internal static string ProjectionsSessionOverview(string apiVersion) =>
        $"api/v{apiVersion}/projections/session-overview";

    internal static string MeasurementValidation(string apiVersion, string measurementId) =>
        $"api/v{apiVersion}/measurements/{Uri.EscapeDataString(measurementId)}/validation";

    internal static string MeasurementConditioning(string apiVersion, string measurementId) =>
        $"api/v{apiVersion}/measurements/{Uri.EscapeDataString(measurementId)}/conditioning";

    internal static string MeasurementCanonicalPublications(string apiVersion, string measurementId) =>
        $"api/v{apiVersion}/measurements/{Uri.EscapeDataString(measurementId)}/canonical-observation/publications";

    internal static string SessionMeasurementContextResolved(string apiVersion, string sessionId, string measurementId) =>
        $"api/v{apiVersion}/sessions/{Uri.EscapeDataString(sessionId)}/measurements/{Uri.EscapeDataString(measurementId)}/context/resolved";

    internal static string AuditFacts(string apiVersion) => $"api/v{apiVersion}/audit/facts";

    internal static string AuditProvenanceLinks(string apiVersion) => $"api/v{apiVersion}/audit/provenance-links";

    internal static string ResourceSemanticConformance(string apiVersion, string resourceId) =>
        $"api/v{apiVersion}/resources/{Uri.EscapeDataString(resourceId)}/semantic-conformance";

    internal static string SurveillanceAlerts(string apiVersion) => $"api/v{apiVersion}/surveillance/alerts";

    internal static string SurveillanceRulesEvaluate(string apiVersion) =>
        $"api/v{apiVersion}/surveillance/rules/evaluate";

    internal static string ClinicalAnalyticsSessionAnalyses(string apiVersion, string treatmentSessionId) =>
        $"api/v{apiVersion}/clinical-analytics/sessions/{Uri.EscapeDataString(treatmentSessionId)}/analyses";

    internal static string ReportingSessionReports(string apiVersion, string treatmentSessionId) =>
        $"api/v{apiVersion}/reporting/sessions/{Uri.EscapeDataString(treatmentSessionId)}/reports";

    internal static string ReportingFinalizeReport(string apiVersion, string reportId) =>
        $"api/v{apiVersion}/reporting/reports/{Uri.EscapeDataString(reportId)}/finalize";

    internal static string ReportingPublishReport(string apiVersion, string reportId) =>
        $"api/v{apiVersion}/reporting/reports/{Uri.EscapeDataString(reportId)}/publication";

    internal static string FinancialCoverageRegistrations(string apiVersion) =>
        $"api/v{apiVersion}/financial/coverage-registrations";

    internal static string FinancialCoverageEligibility(string apiVersion) =>
        $"api/v{apiVersion}/financial/coverage-eligibility";

    internal static string FinancialClaims(string apiVersion) => $"api/v{apiVersion}/financial/claims";

    internal static string FinancialClaimAdjudication(string apiVersion, string claimId) =>
        $"api/v{apiVersion}/financial/claims/{Uri.EscapeDataString(claimId)}/adjudication";

    internal static string FinancialExplanationOfBenefit(string apiVersion) =>
        $"api/v{apiVersion}/financial/explanation-of-benefit";

    internal static string WorkflowStart(string apiVersion) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/start";

    internal static string AdministrationThresholdProfiles(string apiVersion) =>
        $"api/v{apiVersion}/administration-configuration/threshold-profiles";

    internal static string AdministrationRuleSets(string apiVersion) =>
        $"api/v{apiVersion}/administration-configuration/rule-sets";

    internal static string AdministrationRuleSetPublish(string apiVersion, string ruleSetId) =>
        $"api/v{apiVersion}/administration-configuration/rule-sets/{Uri.EscapeDataString(ruleSetId)}/publish";

    /// <summary>Same handler as <see cref="Measurements"/> root POST (MeasurementAcquisition.Api).</summary>
    internal static string MeasurementsIngest(string apiVersion) => $"api/v{apiVersion}/measurements/ingest";

    internal static string PublicationsRetry(string apiVersion, string publicationId) =>
        $"api/v{apiVersion}/publications/{Uri.EscapeDataString(publicationId)}/retry";

    internal static string SessionMeasurementContextUnresolved(string apiVersion, string sessionId, string measurementId) =>
        $"api/v{apiVersion}/sessions/{Uri.EscapeDataString(sessionId)}/measurements/{Uri.EscapeDataString(measurementId)}/context/unresolved";

    internal static string SurveillanceAlertAcknowledge(string apiVersion, string alertId) =>
        $"{SurveillanceAlerts(apiVersion)}/{Uri.EscapeDataString(alertId)}/acknowledge";

    internal static string SurveillanceAlertEscalate(string apiVersion, string alertId) =>
        $"{SurveillanceAlerts(apiVersion)}/{Uri.EscapeDataString(alertId)}/escalate";

    internal static string SurveillanceAlertResolve(string apiVersion, string alertId) =>
        $"{SurveillanceAlerts(apiVersion)}/{Uri.EscapeDataString(alertId)}/resolve";

    internal static string WorkflowAdvance(string apiVersion, string workflowInstanceId) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/{Uri.EscapeDataString(workflowInstanceId)}/advance";

    internal static string WorkflowComplete(string apiVersion, string workflowInstanceId) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/{Uri.EscapeDataString(workflowInstanceId)}/complete";

    internal static string WorkflowFail(string apiVersion, string workflowInstanceId) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/{Uri.EscapeDataString(workflowInstanceId)}/fail";

    internal static string WorkflowCompensation(string apiVersion, string workflowInstanceId) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/{Uri.EscapeDataString(workflowInstanceId)}/compensation";

    internal static string WorkflowManualIntervention(string apiVersion, string workflowInstanceId) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/{Uri.EscapeDataString(workflowInstanceId)}/manual-intervention";

    internal static string WorkflowTimeout(string apiVersion, string workflowInstanceId) =>
        $"api/v{apiVersion}/workflow-orchestrator/workflows/{Uri.EscapeDataString(workflowInstanceId)}/timeout";

    internal static string ProjectionsRebuild(string apiVersion) => $"api/v{apiVersion}/projections/rebuild";

    internal static string ReplayRecoveryRecoveryPlansExecute(string apiVersion) =>
        $"api/v{apiVersion}/replay-recovery/recovery-plans/execute";

    internal static string ReplayRecoveryReplayJobsStart(string apiVersion) =>
        $"api/v{apiVersion}/replay-recovery/replay-jobs/start";

    internal static string ReplayRecoveryReplayJobCheckpoints(string apiVersion, string replayJobId) =>
        $"api/v{apiVersion}/replay-recovery/replay-jobs/{Uri.EscapeDataString(replayJobId)}/checkpoints";

    internal static string ReplayRecoveryReplayJobPause(string apiVersion, string replayJobId) =>
        $"api/v{apiVersion}/replay-recovery/replay-jobs/{Uri.EscapeDataString(replayJobId)}/pause";

    internal static string ReplayRecoveryReplayJobResume(string apiVersion, string replayJobId) =>
        $"api/v{apiVersion}/replay-recovery/replay-jobs/{Uri.EscapeDataString(replayJobId)}/resume";

    internal static string ReplayRecoveryReplayJobComplete(string apiVersion, string replayJobId) =>
        $"api/v{apiVersion}/replay-recovery/replay-jobs/{Uri.EscapeDataString(replayJobId)}/complete";

    internal static string ReplayRecoveryReplayJobFail(string apiVersion, string replayJobId) =>
        $"api/v{apiVersion}/replay-recovery/replay-jobs/{Uri.EscapeDataString(replayJobId)}/fail";
}
