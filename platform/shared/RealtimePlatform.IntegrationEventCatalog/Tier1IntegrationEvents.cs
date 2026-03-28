using BuildingBlocks;

namespace RealtimePlatform.IntegrationEventCatalog;

/// <summary>CLR contracts for catalog §6 Tier 1; payloads are minimal/stable business facts.</summary>
public sealed record MeasurementAcceptedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string DeviceId,
    string SchemaVersion,
    string PayloadHash) : IntegrationEvent(CorrelationId);

public sealed record MeasurementValidatedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ValidationProfileId) : IntegrationEvent(CorrelationId);

public sealed record MeasurementFlaggedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ValidationProfileId,
    string FlagReason) : IntegrationEvent(CorrelationId);

public sealed record MeasurementValidationFailedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ValidationProfileId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record MeasurementValidationQuarantinedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ValidationProfileId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record ValidationRuleTriggeredIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string RuleCode,
    string Outcome) : IntegrationEvent(CorrelationId);

public sealed record SignalConditionedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ConditionedSignalKind) : IntegrationEvent(CorrelationId);

public sealed record SignalQualityCalculatedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    int QualityScorePercent) : IntegrationEvent(CorrelationId);

public sealed record SignalDriftDetectedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ChannelId,
    string Detail) : IntegrationEvent(CorrelationId);

public sealed record SignalDropoutDetectedIntegrationEvent(
    Ulid CorrelationId,
    string MeasurementId,
    string ChannelId) : IntegrationEvent(CorrelationId);

public sealed record TerminologyValidatedIntegrationEvent(
    Ulid CorrelationId,
    string ResourceId) : IntegrationEvent(CorrelationId);

public sealed record TerminologyValidationFailedIntegrationEvent(
    Ulid CorrelationId,
    string ResourceId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record ProfileConformanceValidatedIntegrationEvent(
    Ulid CorrelationId,
    string ResourceId,
    string ProfileUrl) : IntegrationEvent(CorrelationId);

public sealed record ProfileConformanceFailedIntegrationEvent(
    Ulid CorrelationId,
    string ResourceId,
    string ProfileUrl,
    string Reason) : IntegrationEvent(CorrelationId);

/// <summary>Session entered active treatment; <see cref="IntegrationEvent.SessionId"/> / <see cref="IntegrationEvent.PatientId"/> populate the catalog envelope.</summary>
public sealed record DialysisSessionStartedIntegrationEvent : IntegrationEvent
{
    /// <summary>Constructor parameter names align with JSON properties (<c>sessionId</c>, <c>patientId</c>) for §1 envelope round-trip.</summary>
    public DialysisSessionStartedIntegrationEvent(Ulid correlationId, string sessionId, string? patientId)
        : base(correlationId)
    {
        SessionId = sessionId;
        PatientId = patientId;
    }
}

public sealed record DialysisSessionCreatedIntegrationEvent(
    Ulid CorrelationId,
    string TreatmentSessionId) : IntegrationEvent(CorrelationId);

public sealed record PatientAssignedToSessionIntegrationEvent(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string MedicalRecordNumber) : IntegrationEvent(CorrelationId);

public sealed record DialysisSessionCompletedIntegrationEvent(
    Ulid CorrelationId,
    string TreatmentSessionId) : IntegrationEvent(CorrelationId);

public sealed record MeasurementContextResolvedIntegrationEvent(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string MeasurementId) : IntegrationEvent(CorrelationId);

public sealed record MeasurementContextUnresolvedIntegrationEvent(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string MeasurementId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record CanonicalObservationPublishedIntegrationEvent(
    Ulid CorrelationId,
    string ObservationId,
    string FhirResourceReference) : IntegrationEvent(CorrelationId);

public sealed record CanonicalObservationPublicationFailedIntegrationEvent(
    Ulid CorrelationId,
    string ObservationId,
    string MeasurementId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record ReadModelProjectionRebuiltIntegrationEvent(
    Ulid CorrelationId,
    string ProjectionSet,
    int RecordCount) : IntegrationEvent(CorrelationId);

public sealed record AlertRaisedIntegrationEvent(
    Ulid CorrelationId,
    string AlertId,
    string AlertType,
    string Severity) : IntegrationEvent(CorrelationId);

public sealed record AlertAcknowledgedIntegrationEvent(
    Ulid CorrelationId,
    string AlertId,
    string AcknowledgedByUserId) : IntegrationEvent(CorrelationId);

public sealed record AlertEscalatedIntegrationEvent(
    Ulid CorrelationId,
    string AlertId,
    string EscalationDetail) : IntegrationEvent(CorrelationId);

public sealed record AlertResolvedIntegrationEvent(
    Ulid CorrelationId,
    string AlertId,
    string ResolutionNote) : IntegrationEvent(CorrelationId);

public sealed record SessionRiskStateChangedIntegrationEvent(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string RiskLevel) : IntegrationEvent(CorrelationId);

public sealed record DerivedMetricCalculatedIntegrationEvent(
    Ulid CorrelationId,
    string SessionAnalysisId,
    string MetricCode,
    string MetricValue,
    string? Unit) : IntegrationEvent(CorrelationId);

public sealed record AnalyticalConfidenceAssignedIntegrationEvent(
    Ulid CorrelationId,
    string SessionAnalysisId,
    int ConfidenceScorePercent) : IntegrationEvent(CorrelationId);

public sealed record SessionTrendComputedIntegrationEvent(
    Ulid CorrelationId,
    string SessionAnalysisId,
    string TrendSummary) : IntegrationEvent(CorrelationId);

public sealed record SessionAnalysisCompletedIntegrationEvent(
    Ulid CorrelationId,
    string SessionAnalysisId,
    string ModelVersion,
    int DerivedMetricCount) : IntegrationEvent(CorrelationId);

public sealed record SessionReportGeneratedIntegrationEvent(
    Ulid CorrelationId,
    string SessionReportId,
    string TreatmentSessionId,
    string NarrativeVersion) : IntegrationEvent(CorrelationId);

public sealed record SessionReportFinalizedIntegrationEvent(
    Ulid CorrelationId,
    string SessionReportId,
    string TreatmentSessionId) : IntegrationEvent(CorrelationId);

public sealed record DiagnosticReportPublishedIntegrationEvent(
    Ulid CorrelationId,
    string SessionReportId,
    string TreatmentSessionId,
    string? PublicationTargetHint) : IntegrationEvent(CorrelationId);

public sealed record WorkflowStartedIntegrationEvent(
    Ulid CorrelationId,
    string WorkflowInstanceId,
    string WorkflowKind,
    string TreatmentSessionId) : IntegrationEvent(CorrelationId);

public sealed record WorkflowCompletedIntegrationEvent(
    Ulid CorrelationId,
    string WorkflowInstanceId,
    string WorkflowKind,
    int FinalStepOrdinal) : IntegrationEvent(CorrelationId);

public sealed record WorkflowFailedIntegrationEvent(
    Ulid CorrelationId,
    string WorkflowInstanceId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record CompensationTriggeredIntegrationEvent(
    Ulid CorrelationId,
    string WorkflowInstanceId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record ManualInterventionRequestedIntegrationEvent(
    Ulid CorrelationId,
    string WorkflowInstanceId,
    string Detail) : IntegrationEvent(CorrelationId);

public sealed record TimeoutElapsedIntegrationEvent(
    Ulid CorrelationId,
    string WorkflowInstanceId,
    string StepName,
    int StepOrdinal) : IntegrationEvent(CorrelationId);

public sealed record ReplayStartedIntegrationEvent(
    Ulid CorrelationId,
    string ReplayJobId,
    string ReplayMode,
    string ProjectionSetName) : IntegrationEvent(CorrelationId);

public sealed record ReplayCompletedIntegrationEvent(
    Ulid CorrelationId,
    string ReplayJobId,
    string ProjectionSetName,
    int FinalCheckpointSequence) : IntegrationEvent(CorrelationId);

public sealed record ReplayFailedIntegrationEvent(
    Ulid CorrelationId,
    string ReplayJobId,
    string Reason) : IntegrationEvent(CorrelationId);

public sealed record RecoveryPlanExecutedIntegrationEvent(
    Ulid CorrelationId,
    string RecoveryPlanExecutionId,
    string PlanCode,
    string Outcome) : IntegrationEvent(CorrelationId);

public sealed record FacilityConfigurationChangedIntegrationEvent(
    Ulid CorrelationId,
    string TargetFacilityId,
    int ConfigurationRevision) : IntegrationEvent(CorrelationId);

public sealed record RuleSetPublishedIntegrationEvent(
    Ulid CorrelationId,
    string RuleSetId,
    string RuleVersion) : IntegrationEvent(CorrelationId);

public sealed record ThresholdProfileChangedIntegrationEvent(
    Ulid CorrelationId,
    string ThresholdProfileId,
    int ProfileRevision) : IntegrationEvent(CorrelationId);

public sealed record FeatureToggleChangedIntegrationEvent(
    Ulid CorrelationId,
    string FeatureKey,
    bool IsEnabled) : IntegrationEvent(CorrelationId);

/// <summary>Emitted when a platform-wide audit fact is persisted (source systems and central Audit &amp; Provenance).</summary>
public sealed record CriticalAuditEventRecordedIntegrationEvent(
    Ulid CorrelationId,
    string PlatformAuditFactId,
    string EventType) : IntegrationEvent(CorrelationId);

/// <summary>Emitted when two audit facts are linked for provenance (e.g. derivation, supersession).</summary>
public sealed record ProvenanceRecordedIntegrationEvent(
    Ulid CorrelationId,
    string ProvenanceLinkId,
    string FromPlatformAuditFactId,
    string ToPlatformAuditFactId,
    string RelationType) : IntegrationEvent(CorrelationId);

public sealed record PatientCoverageSnapshotRecordedIntegrationEvent(
    Ulid CorrelationId,
    string CoverageRegistrationId) : IntegrationEvent(CorrelationId);

public sealed record CoverageEligibilityOutcomeRecordedIntegrationEvent(
    Ulid CorrelationId,
    string EligibilityInquiryId,
    string OutcomeCode) : IntegrationEvent(CorrelationId);

public sealed record DialysisFinancialClaimSubmittedIntegrationEvent(
    Ulid CorrelationId,
    string FinancialClaimId,
    string TreatmentSessionId) : IntegrationEvent(CorrelationId);

public sealed record ClaimAdjudicationRecordedIntegrationEvent(
    Ulid CorrelationId,
    string FinancialClaimId,
    string ExternalClaimResponseId) : IntegrationEvent(CorrelationId);

public sealed record ExplanationOfBenefitLinkedToSessionIntegrationEvent(
    Ulid CorrelationId,
    string ExplanationOfBenefitId,
    string TreatmentSessionId) : IntegrationEvent(CorrelationId);
