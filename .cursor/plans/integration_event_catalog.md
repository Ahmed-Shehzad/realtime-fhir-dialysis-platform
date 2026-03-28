# Integration Event Catalog - Real-Time FHIR Dialysis Platform

## Purpose
This document defines all **integration events** used across bounded contexts and external integrations.  
Integration events are **durable, versioned, and stable contracts** used to ensure **consistency, orchestration, and communication**.

**Plan governance:** How this catalog is used in roadmaps, domain vs integration boundaries, transactions, and envelope evolution is captured in [`integration_events_plan_considerations.plan.md`](integration_events_plan_considerations.plan.md).

---

## 1. Event Envelope Standard

All integration events must follow:

```json
{
  "eventId": "uuid",
  "eventType": "string",
  "eventVersion": 1,
  "occurredUtc": "timestamp",
  "correlationId": "uuid",
  "causationId": "uuid",
  "workflowId": "uuid",
  "sagaId": "uuid",
  "facilityId": "string",
  "sessionId": "string",
  "patientId": "string",
  "deviceId": "string",
  "partitionKey": "string",
  "payload": {}
}
```

---

## 2. Event Catalog

### Device Context
- DeviceRegisteredIntegrationEvent  
- DeviceSuspendedIntegrationEvent  
- DeviceRetiredIntegrationEvent  
- DeviceCalibrationExpiredIntegrationEvent  
- DeviceAssignmentChangedIntegrationEvent  
- DeviceTrustStatusChangedIntegrationEvent  

### Measurement Acquisition
- MeasurementReceivedIntegrationEvent  
- MeasurementAcceptedIntegrationEvent  
- MeasurementRejectedIntegrationEvent  
- MeasurementQuarantinedIntegrationEvent  
- MeasurementPayloadInvalidIntegrationEvent  
- MeasurementDuplicateDetectedIntegrationEvent  

### Treatment Session
- DialysisSessionCreatedIntegrationEvent  
- DialysisSessionStartedIntegrationEvent  
- DialysisSessionPausedIntegrationEvent  
- DialysisSessionResumedIntegrationEvent  
- DialysisSessionCompletedIntegrationEvent  
- DialysisSessionFailedIntegrationEvent  
- PatientAssignedToSessionIntegrationEvent  
- MeasurementContextResolvedIntegrationEvent  
- MeasurementContextUnresolvedIntegrationEvent  

### Measurement Validation
- MeasurementValidatedIntegrationEvent  
- MeasurementFlaggedIntegrationEvent  
- MeasurementValidationFailedIntegrationEvent  
- MeasurementValidationQuarantinedIntegrationEvent  
- ValidationRuleTriggeredIntegrationEvent  

### Signal Conditioning
- SignalConditionedIntegrationEvent  
- SignalQualityCalculatedIntegrationEvent  
- SignalDriftDetectedIntegrationEvent  
- SignalDropoutDetectedIntegrationEvent  

### Realtime Surveillance
- AlertRaisedIntegrationEvent  
- AlertAcknowledgedIntegrationEvent  
- AlertEscalatedIntegrationEvent  
- AlertResolvedIntegrationEvent  
- SessionRiskStateChangedIntegrationEvent  

### Clinical Analytics
- DerivedMetricCalculatedIntegrationEvent  
- AnalyticalConfidenceAssignedIntegrationEvent  
- SessionTrendComputedIntegrationEvent  
- SessionAnalysisCompletedIntegrationEvent  

### Clinical Interoperability
- CanonicalObservationPublishedIntegrationEvent  
- CanonicalObservationPublicationFailedIntegrationEvent  
- CanonicalSessionPublishedIntegrationEvent  
- CanonicalDiagnosticReportReferencePublishedIntegrationEvent  

### Reporting
- SessionReportGeneratedIntegrationEvent  
- SessionReportFinalizedIntegrationEvent  
- DiagnosticReportPublishedIntegrationEvent  

### Audit & Provenance
- ProvenanceRecordedIntegrationEvent  
- EvidencePackagePreparedIntegrationEvent  
- CriticalAuditEventRecordedIntegrationEvent  

### Terminology & Conformance
- TerminologyValidatedIntegrationEvent  
- TerminologyValidationFailedIntegrationEvent  
- ProfileConformanceValidatedIntegrationEvent  
- ProfileConformanceFailedIntegrationEvent  

### Workflow / Saga
- WorkflowStartedIntegrationEvent  
- WorkflowCompletedIntegrationEvent  
- WorkflowFailedIntegrationEvent  
- CompensationTriggeredIntegrationEvent  
- ManualInterventionRequestedIntegrationEvent  
- TimeoutElapsedIntegrationEvent  

### Replay & Recovery
- ReplayStartedIntegrationEvent  
- ReplayCompletedIntegrationEvent  
- ReplayFailedIntegrationEvent  
- RecoveryPlanExecutedIntegrationEvent  

### Configuration
- FacilityConfigurationChangedIntegrationEvent  
- RuleSetPublishedIntegrationEvent  
- ThresholdProfileChangedIntegrationEvent  
- FeatureToggleChangedIntegrationEvent  

---

## 3. Saga-Critical Events

### Measurement Processing Saga
- MeasurementAcceptedIntegrationEvent  
- MeasurementContextResolvedIntegrationEvent  
- MeasurementValidatedIntegrationEvent  
- SignalConditionedIntegrationEvent  
- CanonicalObservationPublishedIntegrationEvent  

### Session Completion Saga
- DialysisSessionCompletedIntegrationEvent  
- SessionAnalysisCompletedIntegrationEvent  
- SessionReportGeneratedIntegrationEvent  
- DiagnosticReportPublishedIntegrationEvent  

### Failure & Compensation
- MeasurementValidationFailedIntegrationEvent  
- CanonicalObservationPublicationFailedIntegrationEvent  
- WorkflowFailedIntegrationEvent  
- CompensationTriggeredIntegrationEvent  

---

## 4. External Exposure Events

### EHR / FHIR Systems
- CanonicalObservationPublishedIntegrationEvent  
- DiagnosticReportPublishedIntegrationEvent  

### Operations / Monitoring
- AlertRaisedIntegrationEvent  
- MeasurementValidationFailedIntegrationEvent  

### Compliance / Audit
- ProvenanceRecordedIntegrationEvent  
- EvidencePackagePreparedIntegrationEvent  

---

## 5. Key Rules

- Integration events are immutable  
- Must be idempotent  
- Must be versioned  
- Must represent business facts  
- Never expose internal domain logic  
- Always include correlation + causation IDs  

---

## 6. Priority (Initial Implementation)

### Tier 1 (Critical)
- MeasurementAcceptedIntegrationEvent  
- MeasurementValidatedIntegrationEvent  
- SignalConditionedIntegrationEvent  
- DialysisSessionStartedIntegrationEvent  
- CanonicalObservationPublishedIntegrationEvent  
- AlertRaisedIntegrationEvent  

### Tier 2
- DerivedMetricCalculatedIntegrationEvent  
- SessionReportGeneratedIntegrationEvent  
- ProfileConformanceValidatedIntegrationEvent  

### Tier 3
- Replay events  
- Configuration events  
- Projection events  

---

## Summary

Integration events are the backbone of system consistency.

They:
- drive sagas  
- synchronize microservices  
- expose stable contracts externally  
- ensure traceability  

They must be treated as long-lived public contracts.
