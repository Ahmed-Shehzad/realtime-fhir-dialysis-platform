# Real-Time FHIR Dialysis Platform Implementation Plan
**Target audience:** Codex / engineering implementation agents  
**Status:** Working implementation blueprint  
**Primary stack:** Microsoft Azure, .NET/C#, microservices, DDD, Clean Architecture, FHIR canonical platform  
**Architecture principles:** Safety-first, clinical-grade reliability, traceability, C5-aligned controls, event-driven microservices, outbox/inbox idempotency, state-machine orchestration, saga-based long-running transactions

---

# 1. Executive Summary

This document defines the implementation plan for a **real-time, production-ready, reliable dialysis platform** built on **Microsoft Azure** using **.NET/C# microservices**, **Domain-Driven Design**, **Clean Architecture**, **FHIR as the canonical clinical interoperability layer**, and **event-driven asynchronous communication**.

The system must support:

- real-time ingestion of dialysis machine and optical sensor measurements,
- safe validation of measurements at every processing step,
- canonical clinical persistence to FHIR,
- live dashboards and alerting,
- strong auditability and provenance,
- robust handling of long-running workflows and failures through **state machines and sagas**.

A guiding scientific assumption behind the architecture is that optical fluorescence from spent dialysate is **informative but composite**, not a single clean biomarker. Therefore the system must preserve:

- raw measurements,
- conditioned signals,
- derived metrics,
- validation status,
- model version,
- provenance and confidence.

This is a **clinical canonical system**, so the architecture must prioritize **safety, determinism, traceability, and correctness** over convenience.

---

# 2. Vision

## 2.1 Product vision

Build a clinical-grade digital dialysis platform that:

1. ingests dialysis and sensor data in real time,
2. validates every measurement safely and transparently,
3. correlates measurements to the correct patient, treatment, and device context,
4. derives clinically meaningful indicators without hiding uncertainty,
5. persists canonical clinical truth in FHIR,
6. provides live visibility to clinicians and operations teams,
7. supports replay, recovery, and audit evidence,
8. is robust enough for phased clinical production rollout.

## 2.2 Technical vision

Create a modular platform that is:

- **microservice-based** by business capability,
- **DDD-aligned** through explicit bounded contexts,
- **cleanly layered** internally per service,
- **event-driven** through a durable message broker,
- **idempotent** through outbox/inbox patterns,
- **stateful where needed** through explicit workflow state machines,
- **resilient** through retries, dead-lettering, replay, and saga compensation,
- **observable** through logs, traces, metrics, and audit streams.

## 2.3 Clinical safety vision

Every measurement must be:

- trusted,
- attributable,
- validated,
- explainable,
- traceable,
- recoverable,
- and never silently altered.

The system must never collapse raw signal, processed signal, and derived meaning into a single opaque value.

---

# 3. Guiding Principles

## 3.1 Clinical and safety principles

- Safety is the first architectural concern.
- Raw measurements are immutable and preserved.
- Derived metrics must always carry provenance and confidence.
- Validation is a pipeline, not a single check.
- Ambiguous context is quarantined, not auto-guessed.
- Canonical clinical writes are controlled and limited.
- Every clinically relevant action is auditable.

## 3.2 Domain and architecture principles

- Microservices are aligned to bounded contexts, not technical layers.
- FHIR is the canonical interoperability language, not the core domain language.
- Domain model first, integration contracts second, infrastructure third.
- Use asynchronous messaging as default between services.
- Use synchronous APIs only for commands/configuration/query needs that require it.
- Use SignalR/WebSockets for live experience, not durable workflow progression.
- Use webhooks only for external notification/push integration.

## 3.3 Reliability and resilience principles

- All consumers are idempotent.
- All producers use an outbox.
- All critical workflows are resumable.
- Every integration event is versioned.
- Every state transition is explicit and observable.
- Failures should produce either retry, compensation, or quarantine behavior.
- Replay must be possible without violating clinical traceability.

---

# 4. Target Platform and Technology Standards

## 4.1 Core platform

- **Azure Health Data Services FHIR service** for canonical FHIR storage
- **AKS** for .NET microservices
- **Azure Event Hubs and/or Azure Service Bus** as message broker backbone
- **Azure API Management** for managed API exposure
- **Azure Front Door + WAF** for external ingress
- **Microsoft Entra ID** for identity and access
- **Azure Key Vault / Managed HSM** for secrets and keys
- **Azure SQL** for service-owned transactional/operational state
- **Azure Storage** for immutable retention, replay, evidence, and exports
- **Azure Monitor / Application Insights / OpenTelemetry / Grafana**
- **Microsoft Sentinel** for security operations

## 4.2 Application stack

- **.NET 8 / C#**
- ASP.NET Core
- Worker Services
- SignalR
- MediatR or equivalent application dispatch pattern if useful
- EF Core for service-local transactional persistence
- FHIR SDK or HTTP client-based FHIR adapter
- Outbox/inbox implementation in each messaging service

## 4.3 Architectural patterns

- Clean Architecture in every microservice
- Domain-Driven Design
- Outbox pattern
- Inbox/idempotent consumer pattern
- Saga orchestration for long-running business workflows
- Explicit state machines for sessions and workflow lifecycles
- CQRS where it clearly reduces risk or read complexity

---

# 5. Scope of the Initial Program

## 5.1 In scope

- Trusted device onboarding
- Real-time measurement ingestion
- Measurement/session correlation
- Validation pipeline
- Signal conditioning
- Alerting and real-time session surveillance
- Canonical FHIR write orchestration
- Session reporting
- Audit/provenance
- Read models and live dashboard updates
- Replay and recovery
- Configuration and governance foundation
- Saga and state-machine orchestration patterns

## 5.2 Out of scope for initial release

- Advanced ML-based clinical predictions
- Broad research portal
- Population-scale analytics warehouse
- Multi-tenant external commercialization features
- Deep patient self-service
- Non-dialysis modality expansion

---

# 6. Program-Level Milestones

## Milestone 0 - Foundations and control baseline
**Goal:** establish delivery, security, governance, and architecture standards.

### Deliverables
- architecture decision records repository
- coding standards
- service template for Clean Architecture microservices
- outbox/inbox shared library or reference implementation
- observability baseline
- identity and access baseline
- CI/CD templates
- IaC landing zone
- event naming/versioning standard
- FHIR profile governance baseline
- bounded context catalog v1
- state machine and saga strategy

## Milestone 1 - Trusted ingest and session identity
**Goal:** safely receive measurements and attach them to correct treatment sessions.

### Deliverables
- Device Registry Service
- Measurement Acquisition Service
- Session Service
- raw measurement contract
- device trust registry
- session lifecycle state machine
- first end-to-end ingestion topic contracts
- quarantine/dead-letter strategy
- audit hooks for ingestion and correlation

## Milestone 2 - Validation pipeline and technical signal conditioning
**Goal:** ensure measurements are safe and processable.

### Deliverables
- Measurement Validation Service
- Signal Conditioning Service
- validation rule model
- validation state model
- signal quality scoring
- basic replay capability
- alert-ready conditioned signal events
- validation provenance model

## Milestone 3 - Canonical clinical persistence
**Goal:** create controlled canonical FHIR records from validated domain events.

### Deliverables
- Clinical Interoperability Service
- Terminology and Conformance Service
- canonical FHIR mapping specifications
- FHIR transaction bundle builder
- canonical write policy
- observation/procedure/session publication flow
- FHIR failure/retry/quarantine handling

## Milestone 4 - Real-time surveillance and clinician visibility
**Goal:** provide live treatment visibility and actionable alerts.

### Deliverables
- Realtime Clinical Surveillance Service
- Realtime Delivery Service
- Query/Read Model Service
- SignalR hub contracts
- dashboard read models
- alert lifecycle state machine
- notification integration baseline

## Milestone 5 - Session reports and orchestration maturity
**Goal:** complete end-of-session reporting and long-running orchestration.

### Deliverables
- Reporting Service
- Workflow Orchestrator / Saga Coordinator
- treatment completion saga
- session summary generation
- diagnostic report publication
- operator intervention workflows
- compensation handling playbooks

## Milestone 6 - Recovery, DR, and production hardening
**Goal:** production readiness and resilient operations.

### Deliverables
- Replay and Recovery Service
- failover playbooks
- DR test evidence
- chaos/replay/regression suite
- audit evidence pack generation
- performance and scaling tests
- security hardening completion
- release gates for clinical-critical services

---

# 7. Iterative Delivery Strategy

Implementation should proceed **iteratively and vertically**, not by horizontal technical layer alone.

Each iteration should aim to produce:
- one or more bounded contexts,
- an executable service,
- events/contracts,
- persistence,
- observability,
- tests,
- documented states,
- and a runnable scenario.

## Iteration order

1. platform foundations
2. device trust
3. measurement acquisition
4. treatment session
5. audit/provenance
6. validation
7. signal conditioning
8. terminology/conformance
9. clinical interoperability
10. real-time surveillance
11. realtime delivery
12. query/read models
13. reporting
14. replay/recovery
15. workflow orchestration and mature sagas
16. hardening and production readiness

---

# 8. Bounded Contexts and Microservices

> Note: one microservice generally implements one bounded context in the safety-critical path. Some supporting contexts may share deployment early, but the conceptual boundaries should remain strict.

---

## 8.1 Device Onboarding and Trust Context
**Microservice:** `DeviceRegistryService`

### Purpose
Own the lifecycle and trustworthiness of devices.

### Responsibilities
- device registration
- firmware compatibility
- calibration status metadata
- assignment eligibility
- trust/suspension/retirement state
- credential metadata

### Aggregate roots
- `Device`
- `CalibrationRecord`
- `DeviceAssignment`

### Entities
- `Device`
- `FirmwareProfile`
- `CalibrationRecord`
- `CertificateBinding`
- `SiteAssignment`

### Value objects
- `DeviceId`
- `ManufacturerInfo`
- `FirmwareVersion`
- `CalibrationWindow`
- `TrustState`
- `StationId`
- `SiteId`

### Commands
- RegisterDevice
- SuspendDevice
- RetireDevice
- AssignDeviceToSite
- UpdateCalibrationStatus

### Domain events
- DeviceRegistered
- DeviceSuspended
- DeviceRetired
- DeviceCalibrationExpired
- DeviceAssignmentChanged

### Deliverables
- REST API for device administration
- persistence model
- trust validation contract for downstream services
- event contracts
- audit integration

---

## 8.2 Measurement Acquisition Context
**Microservice:** `MeasurementAcquisitionService`

### Purpose
Receive raw telemetry and normalize it into canonical acquisition events.

### Responsibilities
- inbound protocol handling
- schema validation
- signature / origin checks
- normalization
- initial accept/reject decision
- raw data persistence
- publish accepted/rejected integration events

### Aggregate roots
- `AcquisitionBatch`
- `RawMeasurementEnvelope`

### Entities
- `RawMeasurement`
- `AcquisitionEnvelope`
- `IngestionAttempt`
- `AcquisitionRejection`

### Value objects
- `MeasurementId`
- `DeviceTimestamp`
- `AcquisitionTimestamp`
- `PayloadHash`
- `SchemaVersion`
- `AcquisitionChannel`
- `MeasurementType`

### Commands
- ReceiveMeasurementPayload
- AcceptMeasurement
- RejectMeasurement

### Domain events
- MeasurementReceived
- MeasurementAccepted
- MeasurementRejected
- PayloadSchemaInvalid

### Deliverables
- ingestion endpoints
- broker publishing
- raw storage model
- invalid payload quarantine
- outbox-enabled transaction publishing
- consumer contract tests

---

## 8.3 Treatment Session Context
**Microservice:** `TreatmentSessionService`

### Purpose
Own dialysis session identity, lifecycle, and patient/device/station correlation.

### Responsibilities
- start/pause/resume/end session
- assign patient and device context
- resolve measurement-to-session mapping
- maintain active treatment state
- expose session lookup
- manage context ambiguity

### Aggregate roots
- `DialysisSession`
- `SessionAssignment`

### Entities
- `DialysisSession`
- `PatientAssignment`
- `DeviceSessionLink`
- `EncounterLink`
- `ProcedureLink`

### Value objects
- `SessionId`
- `PatientId`
- `EncounterId`
- `ProcedureId`
- `StationId`
- `SessionState`
- `CorrelationStatus`

### Commands
- StartDialysisSession
- PauseDialysisSession
- ResumeDialysisSession
- EndDialysisSession
- AssignPatientToSession
- AttachMeasurementToSession
- MarkMeasurementContextUnresolved

### Domain events
- DialysisSessionStarted
- DialysisSessionPaused
- DialysisSessionResumed
- DialysisSessionEnded
- PatientAssignedToSession
- MeasurementContextResolved
- MeasurementContextUnresolved

### Deliverables
- session lifecycle API
- correlation engine
- session state machine
- unresolved context quarantine flow
- events and read models for downstream contexts

---

## 8.4 Measurement Validation Context
**Microservice:** `MeasurementValidationService`

### Purpose
Determine whether a measurement is safe, plausible, and processable.

### Responsibilities
- rule evaluation
- range checks
- calibration dependency checks
- temporal plausibility checks
- data completeness checks
- validation state assignment
- quarantine/flag/fail/safe outcomes

### Aggregate roots
- `MeasurementValidation`
- `ValidationRuleSet`

### Entities
- `MeasurementValidation`
- `ValidationRule`
- `ValidationDecision`
- `ValidationExceptionCase`

### Value objects
- `ValidationState`
- `ValidationSeverity`
- `RuleCode`
- `AllowedRange`
- `PlausibilityWindow`
- `QualityFlag`

### Commands
- ValidateMeasurement
- FlagMeasurement
- FailMeasurement
- ApproveMeasurement

### Domain events
- MeasurementValidated
- MeasurementFlagged
- MeasurementFailedValidation
- ValidationRuleTriggered

### Deliverables
- validation engine
- rule repository
- state model
- rule execution audit
- versioned validation policy
- integration events for downstream conditioning

---

## 8.5 Signal Conditioning Context
**Microservice:** `SignalConditioningService`

### Purpose
Transform validated measurements into conditioned signals suitable for analytics, dashboards, and canonical publication.

### Responsibilities
- smoothing
- de-noising
- missing-data handling
- outlier handling
- signal quality computation
- technical derived metrics
- drift/dropout detection

### Aggregate roots
- `SignalWindow`
- `QualityAssessment`

### Entities
- `SignalPoint`
- `SignalWindow`
- `ConditionedSignal`
- `QualityAssessment`
- `DriftAssessment`

### Value objects
- `SignalValue`
- `ChannelId`
- `WindowRange`
- `QualityScore`
- `DriftState`
- `ConditioningMethodVersion`

### Commands
- ConditionSignal
- AssessSignalQuality
- DetectDrift
- MarkSignalDropout

### Domain events
- SignalConditioned
- SignalQualityCalculated
- SignalDriftDetected
- SignalDropoutDetected

### Deliverables
- streaming consumer/producer
- quality scoring model
- temporal window processor
- conditioned signal event contracts
- technical metrics store

---

## 8.6 Realtime Clinical Surveillance Context
**Microservice:** `RealtimeSurveillanceService`

### Purpose
Monitor active sessions and raise actionable alerts from live operational and clinical-support rules.

### Responsibilities
- alert rule execution
- active alert state management
- deduplication and suppression
- escalation
- acknowledgement
- session risk state tracking

### Aggregate roots
- `Alert`
- `SessionRiskState`

### Entities
- `Alert`
- `AlertCondition`
- `EscalationPolicy`
- `AlertAcknowledgement`
- `SessionRiskState`

### Value objects
- `AlertId`
- `AlertSeverity`
- `AlertState`
- `AlertSource`
- `EscalationStep`
- `SuppressionWindow`

### Commands
- EvaluateAlertRules
- RaiseAlert
- AcknowledgeAlert
- ResolveAlert
- EscalateAlert

### Domain events
- AlertRaised
- AlertAcknowledged
- AlertResolved
- AlertEscalated
- SessionRiskStateChanged

### Deliverables
- rules engine
- alert state machine
- integration with realtime delivery
- operator acknowledgement API
- suppression and escalation policy model

---

## 8.7 Clinical Analytics Context
**Microservice:** `ClinicalAnalyticsService`

### Purpose
Compute session-level derived metrics and clinically meaningful but model-based indicators.

### Responsibilities
- trend summarization
- derived optical metrics
- confidence scoring
- session comparison
- model version capture
- derived metric publishing

### Aggregate roots
- `SessionAnalysis`
- `DerivedMetricSet`

### Entities
- `SessionAnalysis`
- `DerivedMetric`
- `AnalyticalModel`
- `ConfidenceAssessment`

### Value objects
- `MetricCode`
- `MetricValue`
- `ConfidenceScore`
- `ModelVersion`
- `AnalysisWindow`
- `InterpretationStatus`

### Commands
- AnalyzeSession
- CalculateDerivedMetric
- AssignConfidenceScore

### Domain events
- DerivedMetricCalculated
- SessionTrendComputed
- AnalyticalConfidenceAssigned

### Deliverables
- derived metric engine
- confidence scoring
- model versioning hooks
- session comparison logic
- clinically safe derived metric events

---

## 8.8 Clinical Interoperability Context
**Microservice:** `ClinicalInteroperabilityService`

### Purpose
Translate domain facts into canonical FHIR resources and manage controlled clinical writes.

### Responsibilities
- FHIR mapping
- profile coordination
- transaction bundle composition
- conditional create/update
- canonical write governance
- failure/retry/quarantine for FHIR persistence

### Aggregate roots
- `CanonicalPublicationRequest`
- `CanonicalResourceBatch`

### Entities
- `CanonicalObservationProjection`
- `CanonicalSessionProjection`
- `PublicationAttempt`
- `BundleAssembly`

### Value objects
- `ResourceType`
- `FHIRProfileUrl`
- `PublicationStatus`
- `BundleId`
- `CanonicalKey`

### Commands
- PublishCanonicalObservation
- PublishCanonicalSessionState
- PublishProcedureData
- PublishDiagnosticReportReference

### Domain events
- CanonicalObservationCreated
- CanonicalObservationFailed
- CanonicalSessionUpdated
- CanonicalPublicationRetried

### Deliverables
- FHIR adapter
- bundle builder
- mapping specifications
- canonical write policies
- resilient retry strategy
- publication audit trail

---

## 8.9 Reporting Context
**Microservice:** `ReportingService`

### Purpose
Assemble session summaries and diagnostic reports for clinicians and downstream systems.

### Responsibilities
- session summary composition
- narrative generation
- evidence collection
- report finalization
- report publication coordination

### Aggregate roots
- `SessionReport`

### Entities
- `SessionReport`
- `ReportSection`
- `SupportingEvidence`
- `NarrativeTemplate`

### Value objects
- `ReportId`
- `ReportStatus`
- `EvidenceReference`
- `NarrativeVersion`

### Commands
- GenerateSessionReport
- FinalizeSessionReport
- RegenerateSessionReport

### Domain events
- DiagnosticReportGenerated
- DiagnosticReportFinalized
- SessionSummaryPublished

### Deliverables
- session report generator
- reporting state machine
- report evidence linker
- diagnostic report publication requests

---

## 8.10 Audit and Provenance Context
**Microservice:** `AuditProvenanceService`

### Purpose
Maintain immutable traceability across actions, processing steps, and derived outputs.

### Responsibilities
- record audit events
- build provenance chains
- capture causation/correlation
- export evidence packs
- support compliance and investigations

### Aggregate roots
- `AuditTrail`
- `ProvenanceRecord`

### Entities
- `AuditEntry`
- `ProvenanceRecord`
- `EvidenceExport`
- `ProcessingTrace`

### Value objects
- `AuditId`
- `CorrelationId`
- `CausationId`
- `ActorId`
- `TraceStep`
- `EvidencePackageId`

### Commands
- RecordAuditEntry
- RecordProvenance
- ExportEvidencePackage

### Domain events
- AuditRecorded
- ProvenanceRecorded
- EvidencePackagePrepared

### Deliverables
- append-only audit persistence
- provenance event subscriptions
- evidence export API
- trace visualization/query support

---

## 8.11 Terminology and Conformance Context
**Microservice:** `TerminologyConformanceService`

### Purpose
Govern semantic correctness across units, codes, profiles, and conformance rules.

### Responsibilities
- terminology mapping
- UCUM/unit validation
- FHIR profile conformance checks
- local coding rules
- semantic validation services for interoperability and reporting

### Aggregate roots
- `ConformancePolicy`
- `TerminologyMappingSet`

### Entities
- `ConformanceRule`
- `ValueSetBinding`
- `CodeMapping`
- `UnitMapping`

### Value objects
- `CodeSystemUri`
- `CodeValue`
- `UnitCode`
- `ProfileUrl`
- `ConformanceOutcome`

### Commands
- ValidateTerminology
- ValidateProfileConformance
- MapCode
- ValidateUnit

### Domain events
- ConformanceValidated
- ConformanceRejected
- TerminologyMapped

### Deliverables
- terminology API
- profile validation service
- mapping registry
- error contracts for invalid semantics

---

## 8.12 Realtime Delivery Context
**Microservice:** `RealtimeDeliveryService`

### Purpose
Push live session, alert, and dashboard updates to subscribers.

### Responsibilities
- SignalR hub management
- websocket groups
- live feed payload shaping
- subscriber authorization integration
- user/session role-based channel segmentation

### Aggregate roots
- `RealtimeSubscription`

### Entities
- `SubscriptionConnection`
- `RealtimeGroup`
- `LiveViewProjection`

### Value objects
- `ConnectionId`
- `GroupId`
- `SubscriptionScope`
- `DeliveryChannel`
- `ViewType`

### Commands
- ConnectSubscriber
- SubscribeToSession
- UnsubscribeFromSession
- PushRealtimeUpdate

### Domain events
- SubscriberConnected
- SubscriberDisconnected
- RealtimeUpdatePublished

### Deliverables
- SignalR hub
- subscriber authorization rules
- realtime payload contracts
- dashboard push adapters

---

## 8.13 Query and Read Model Context
**Microservice:** `QueryReadModelService`

### Purpose
Provide optimized read models for operational dashboards and clinician workflows.

### Responsibilities
- denormalized session views
- alert lists
- live station/facility boards
- timeline projections
- query APIs

### Aggregate roots
- none in heavy DDD sense; projection-oriented context

### Entities
- `SessionOverviewProjection`
- `AlertProjection`
- `StationBoardProjection`
- `PatientSessionTimeline`

### Value objects
- `ProjectionVersion`
- `ViewFilter`
- `SortOrder`

### Commands
- RebuildProjection
- UpdateSessionProjection
- UpdateAlertProjection

### Domain events
- ProjectionUpdated
- ProjectionRebuilt

### Deliverables
- query API
- projection handlers
- projection rebuild support
- dashboard-optimized DTOs

---

## 8.14 Administration and Configuration Context
**Microservice:** `AdministrationConfigurationService`

### Purpose
Manage operational rules, configuration, thresholds, flags, and environment-specific policies.

### Responsibilities
- site configuration
- rule set management
- threshold profiles
- feature flags
- rollout configuration
- environment policy publication

### Aggregate roots
- `FacilityConfiguration`
- `RuleSet`
- `ThresholdProfile`

### Entities
- `FacilityConfiguration`
- `RuleDefinition`
- `ThresholdDefinition`
- `FeatureToggle`

### Value objects
- `FacilityId`
- `RuleVersion`
- `ThresholdValue`
- `FeatureFlagKey`
- `EffectiveDateRange`

### Commands
- UpdateFacilityConfiguration
- PublishRuleSet
- ChangeThresholdProfile
- ToggleFeature

### Domain events
- FacilityConfigurationChanged
- RuleSetPublished
- ThresholdProfileChanged
- FeatureToggled

### Deliverables
- admin API
- change audit
- runtime configuration propagation
- validation of config before activation

---

## 8.15 Replay and Recovery Context
**Microservice:** `ReplayRecoveryService`

### Purpose
Reprocess historical events and rebuild system state safely after faults, rule changes, or incident recovery.

### Responsibilities
- replay orchestration
- projection rebuilds
- re-validation pipelines
- FHIR republish coordination where allowed
- recovery controls and evidence

### Aggregate roots
- `ReplayJob`
- `RecoveryPlan`

### Entities
- `ReplayJob`
- `ReplayWindow`
- `RecoveryPlan`
- `ReplayCheckpoint`

### Value objects
- `ReplayJobId`
- `ReplayMode`
- `ReplayRange`
- `CheckpointToken`
- `RecoveryStatus`

### Commands
- StartReplay
- PauseReplay
- ResumeReplay
- CancelReplay
- ExecuteRecoveryPlan

### Domain events
- ReplayStarted
- ReplayCheckpointReached
- ReplayCompleted
- RecoveryPlanExecuted

### Deliverables
- replay control API
- checkpointing
- replay audit/provenance
- deterministic reprocessing harness

---

## 8.16 Workflow Orchestration Context
**Microservice:** `WorkflowOrchestratorService`

### Purpose
Coordinate long-running multi-step workflows across bounded contexts via state machines and sagas.

### Responsibilities
- saga orchestration
- workflow state persistence
- timeout handling
- retries and compensation triggering
- operator intervention state
- cross-context process visibility

### Aggregate roots
- `WorkflowInstance`
- `SagaInstance`

### Entities
- `WorkflowInstance`
- `SagaStep`
- `CompensationAction`
- `TimeoutMarker`
- `ManualInterventionRequest`

### Value objects
- `WorkflowId`
- `SagaId`
- `StepId`
- `WorkflowState`
- `SagaState`
- `CompensationStatus`

### Commands
- StartWorkflow
- AdvanceWorkflow
- TriggerCompensation
- CompleteWorkflow
- FailWorkflow
- RequestManualIntervention

### Domain events
- WorkflowStarted
- WorkflowAdvanced
- WorkflowFailed
- CompensationTriggered
- ManualInterventionRequested
- WorkflowCompleted

### Deliverables
- state machine engine or orchestration module
- saga state persistence
- compensating action registry
- workflow monitoring API
- incident/operator intervention flows

---

# 9. Context Map

## 9.1 Primary flow

```text
Device Registry
    ↓
Measurement Acquisition
    ↓
Treatment Session
    ↓
Measurement Validation
    ↓
Signal Conditioning
    ↓
Clinical Analytics
    ↓
Clinical Interoperability
    ↓
Reporting
```

## 9.2 Cross-cutting contexts

- Audit and Provenance
- Terminology and Conformance
- Administration and Configuration
- Query and Read Model
- Realtime Delivery
- Replay and Recovery
- Workflow Orchestration

## 9.3 Anti-corruption boundary

The strongest anti-corruption boundary is between:
- internal domain contexts
- and FHIR canonical representation

**Rule:** domain services speak domain language first. Only the Clinical Interoperability context speaks FHIR natively.

---

# 10. Clean Architecture Template for Every Microservice

Each microservice should follow the same high-level structure:

```text
src/
  Service.Api/
  Service.Application/
  Service.Domain/
  Service.Infrastructure/
  Service.Contracts/
tests/
  Service.UnitTests/
  Service.IntegrationTests/
  Service.ContractTests/
  Service.ArchitectureTests/
```

## 10.1 Domain layer
Contains:
- aggregates
- entities
- value objects
- domain services
- domain events
- invariants

## 10.2 Application layer
Contains:
- commands/queries
- handlers
- orchestration/use cases
- transaction coordination
- outbox write
- inbox deduplication
- validation orchestration

## 10.3 Infrastructure layer
Contains:
- EF Core persistence
- message broker adapters
- FHIR adapters
- SignalR adapters
- external client integrations
- storage adapters
- configuration providers

## 10.4 API layer
Contains:
- HTTP endpoints
- health checks
- admin endpoints
- diagnostics endpoints
- webhook endpoints if applicable

---

# 11. Messaging Strategy

## 11.1 Broker use

Use the message broker for:
- integration events
- workflow progression
- durable async processing
- retries
- replay
- decoupling
- scaling

## 11.2 Message categories

### Domain events
Internal to bounded context.
Example:
- `MeasurementValidatedDomainEvent`

### Integration events
Published for other services.
Example:
- `MeasurementValidatedIntegrationEvent`

### Commands
Use sparingly for explicit work requests between services when orchestration is needed.
Example:
- `GenerateSessionReport`

## 11.3 Message contract standards

Every integration event should include:
- message id
- message type
- version
- correlation id
- causation id
- produced timestamp
- tenant/site context
- session id if relevant
- patient id if safe and required
- device id if relevant
- validation state
- payload

---

# 12. Outbox / Inbox / Idempotency Design

## 12.1 Producer pattern
Within one transaction:
1. update aggregate state
2. persist domain state
3. persist outbox records
4. commit transaction

Outbox publisher then:
1. reads unprocessed outbox messages
2. publishes to broker
3. marks as published with retry metadata

## 12.2 Consumer pattern
Each consumer:
1. receives message
2. checks inbox/dedup store by message id
3. if already processed, no-op safely
4. execute handler
5. persist state + inbox record atomically
6. acknowledge message

## 12.3 Rules
- never publish to broker directly from domain logic
- never let duplicate messages mutate state twice
- integration events must be immutable
- preserve event version compatibility across releases

---

# 13. Validation and Measurement Safety Pipeline

Validation is an architectural pipeline:

```text
Trusted Device
    ↓
Acquisition Validation
    ↓
Session Correlation Validation
    ↓
Measurement Validation
    ↓
Signal Conditioning Validation
    ↓
Clinical Analytics Validation
    ↓
Terminology/Conformance Validation
    ↓
FHIR Canonical Validation
    ↓
Report Validation
```

## 13.1 Measurement states
Suggested lifecycle:

```text
RECEIVED
→ ACCEPTED
→ CONTEXT_RESOLVED
→ VALIDATED / FLAGGED / FAILED
→ CONDITIONED
→ ANALYZED
→ CANONICALIZED
→ REPORTED
```

## 13.2 Safety rules
- never overwrite raw measurement
- never silently auto-correct
- rejected vs flagged must be explicit
- confidence must travel with derived metrics
- unresolved session context must be quarantined
- failed FHIR publication must not lose upstream evidence

---

# 14. State Machines

State machines should be explicit and code-defined, persisted, and observable.

## 14.1 Dialysis session state machine

### States
- Created
- Prepared
- Active
- Paused
- Completing
- Completed
- Failed
- Cancelled

### Transitions
- Created -> Prepared
- Prepared -> Active
- Active -> Paused
- Paused -> Active
- Active -> Completing
- Completing -> Completed
- Any allowed terminal failure path -> Failed
- Prepared/Active/Paused -> Cancelled under controlled rules

### Events
- SessionPrepared
- DialysisSessionStarted
- DialysisSessionPaused
- DialysisSessionResumed
- SessionCompletionTriggered
- DialysisSessionEnded
- SessionFailed
- SessionCancelled

## 14.2 Measurement processing state machine

### States
- Received
- Rejected
- Accepted
- ContextResolved
- ContextUnresolved
- Validated
- Flagged
- FailedValidation
- Conditioned
- Analyzed
- Canonicalized
- ReportingReady
- Archived

### Transitions
- Received -> Accepted or Rejected
- Accepted -> ContextResolved or ContextUnresolved
- ContextResolved -> Validated / Flagged / FailedValidation
- Validated -> Conditioned
- Conditioned -> Analyzed
- Analyzed -> Canonicalized
- Canonicalized -> ReportingReady
- ReportingReady -> Archived

## 14.3 Alert lifecycle state machine

### States
- Detected
- Active
- Suppressed
- Acknowledged
- Escalated
- Resolved
- Closed

### Transitions
- Detected -> Active
- Active -> Suppressed
- Active -> Acknowledged
- Active -> Escalated
- Acknowledged -> Resolved
- Escalated -> Resolved
- Resolved -> Closed

## 14.4 Report lifecycle state machine

### States
- Pending
- Building
- AwaitingDependencies
- ReadyForReview
- Finalized
- Published
- Failed

### Transitions
- Pending -> Building
- Building -> AwaitingDependencies
- AwaitingDependencies -> Building
- Building -> ReadyForReview
- ReadyForReview -> Finalized
- Finalized -> Published
- Any state -> Failed if unrecoverable

## 14.5 Workflow/saga state machine
Each saga should also have states:
- Started
- InProgress
- WaitingForEvent
- WaitingForTimeout
- Compensating
- Completed
- Failed
- ManualInterventionRequired

---

# 15. Saga Design for Long-Running Transactions

Long-running workflows must not rely on distributed transactions. Use sagas.

## 15.1 Saga principles
- local transaction per service
- publish event after commit via outbox
- next step triggered asynchronously
- compensating actions for business rollback
- explicit timeouts and retries
- operator intervention if compensation cannot complete safely

## 15.2 Candidate sagas

### A. Session initiation saga
**Purpose:** establish a valid active treatment context.

#### Steps
1. verify device trust
2. create session
3. assign patient
4. link encounter/procedure context
5. publish session active event

#### Compensation
- cancel session
- mark device unavailable for session start
- emit audit failure event

### B. Measurement-to-canonical-observation saga
**Purpose:** move accepted measurement through validation to canonical publication.

#### Steps
1. measurement accepted
2. session correlation resolved
3. validation completed
4. conditioning completed
5. terminology/conformance validated
6. canonical FHIR publication executed

#### Compensation
- quarantine measurement
- mark publication failed
- issue manual review request if needed
- maintain evidence trail; do not destroy raw measurement

### C. Session completion/reporting saga
**Purpose:** conclude treatment and publish session report.

#### Steps
1. session end triggered
2. ensure final measurements processed
3. compute final derived metrics
4. generate report
5. publish canonical report references
6. mark session completed

#### Compensation
- revert report status to failed
- hold session in completing state
- request manual intervention
- republish missing dependencies

### D. Replay/recovery saga
**Purpose:** safely reprocess historical windows after incident or rule change.

#### Steps
1. create replay plan
2. checkpoint source range
3. replay messages
4. rebuild projections
5. re-run publication where allowed
6. compare expected vs actual
7. close replay

#### Compensation
- stop replay
- rollback projection rebuild where possible
- retain evidence of partial replay
- flag data review requirement

## 15.3 Saga coordinator responsibilities
- persist saga state
- correlate events to workflow instance
- handle timeouts
- trigger retries
- trigger compensation
- emit workflow telemetry and audit

---

# 16. Canonical FHIR Governance

## 16.1 Canonical write rule
FHIR canonical writes should be controlled by the **Clinical Interoperability Service** only, or by a very small governed set of canonical-writing services.

## 16.2 Domain vs FHIR language
- internal contexts: raw measurement, conditioned signal, derived metric, session analysis
- FHIR boundary: Observation, Device, Procedure, Encounter, DiagnosticReport, Provenance, AuditEvent

## 16.3 Publication requirements
Every canonical resource creation should include:
- source correlation ids
- device references
- session references
- validation status where appropriate
- provenance chain linkage
- mapping version

---

# 17. Cross-Cutting Nonfunctional Requirements

## 17.1 Security
- Entra ID
- managed identities
- no embedded secrets
- least privilege
- API and broker authz
- signed webhooks
- private networking where possible

## 17.2 Observability
Every service must emit:
- structured logs
- distributed traces
- metrics
- health checks
- message lag metrics
- failure counts
- outbox/inbox metrics
- state-machine transition metrics

## 17.3 Auditability
Every critical action must be auditable:
- who
- what
- when
- why
- source
- affected entities
- outcome
- trace ids

## 17.4 Reliability
- retries with backoff
- poison message handling
- DLQ
- replay support
- circuit breakers
- timeout policies
- graceful degradation for dashboards

## 17.5 Performance
- low-latency ingest path
- bounded synchronous calls in critical path
- projection-based reads
- broker-based scaling for hot paths

---

# 18. Per-Service Engineering Checklist

Every service implementation must include:

- Clean Architecture skeleton
- bounded context documentation
- domain model with aggregate roots
- command/query handlers
- outbox support
- inbox/idempotent consumer support
- structured logging
- tracing
- metrics
- health endpoints
- dead-letter strategy
- contract tests
- unit tests
- integration tests
- architecture tests
- Helm chart / deployment manifest
- configuration schema validation
- ADR updates where needed
- runbook entry
- ownership metadata

---

# 19. Detailed Delivery Plan by Iteration

## Iteration 1 - Shared foundation
### Deliverables
- solution and repo strategy
- service template
- shared event contract guidelines
- shared observability package
- shared outbox/inbox package or template
- CI/CD baseline
- architecture test rules
- base state machine abstraction
- saga orchestration skeleton

## Iteration 2 - Device Registry Service
### Deliverables
- domain model
- REST API
- DB schema
- outbox publishing
- audit support
- trust lookup contract

## Iteration 3 - Measurement Acquisition Service
### Deliverables
- ingress endpoints/adapters
- raw measurement persistence
- accept/reject flow
- broker publishing
- DLQ handling
- measurement received state machine start

## Iteration 4 - Treatment Session Service
### Deliverables
- session state machine
- session start/end API
- patient/device assignment
- context resolution
- correlation events

## Iteration 5 - AuditProvenanceService
### Deliverables
- audit event ingestion
- provenance record persistence
- evidence export baseline
- trace model

## Iteration 6 - Measurement Validation Service
### Deliverables
- validation engine
- rules versioning
- validation states
- flagged/failed outcomes
- rule execution audit

## Iteration 7 - Signal Conditioning Service
### Deliverables
- stream processors
- signal quality model
- drift/dropout logic
- conditioned signal events

## Iteration 8 - TerminologyConformanceService
### Deliverables
- unit/code validation
- conformance API
- profile rule registry
- semantic validation contracts

## Iteration 9 - ClinicalInteroperabilityService
### Deliverables
- mapping engine
- FHIR bundle publishing
- canonical retry handling
- publication state machine
- canonical publication audit

## Iteration 10 - QueryReadModelService
### Deliverables
- session overview projection
- alert projection
- dashboard query endpoints
- projection rebuild support

## Iteration 11 - RealtimeSurveillanceService
### Deliverables
- alert rules
- alert state machine
- acknowledgement endpoints
- escalation flows

## Iteration 12 - RealtimeDeliveryService
### Deliverables
- SignalR hubs
- live update contracts
- subscriber group authorization
- alert/session feed push

## Iteration 13 - ClinicalAnalyticsService
### Deliverables
- derived metric engine
- confidence scoring
- analysis events
- model version metadata

## Iteration 14 - ReportingService
### Deliverables
- report generation
- report lifecycle state machine
- evidence linking
- report publication flow

## Iteration 15 - WorkflowOrchestratorService
### Deliverables
- saga persistence
- workflow APIs
- session completion saga
- canonical publication saga
- manual intervention flow

## Iteration 16 - ReplayRecoveryService
### Deliverables
- replay plan execution
- checkpointing
- projection rebuilds
- deterministic replay tests

## Iteration 17 - AdministrationConfigurationService
### Deliverables
- config/rule APIs
- threshold and feature flag publication
- validation before activation

## Iteration 18 - Hardening and pre-production
### Deliverables
- performance testing
- failover tests
- replay drills
- audit evidence packs
- release gates
- runbooks
- operational acceptance criteria

## Iteration 19 - Contract tests and bounded context catalog
### Deliverables
- automated Tier 1 integration event transport serialization contract tests
- bounded context catalog document (operators and implementers)
- cross-links from integration event contract documentation

---

# 20. Testing Strategy

## 20.1 Unit tests
- aggregates
- invariants
- value object correctness
- state transitions
- validation rules

## 20.2 Integration tests
- DB + outbox
- broker publishing/consumption
- FHIR adapter behavior
- SignalR push behavior
- replay pipeline

## 20.3 Contract tests
- event schema compatibility
- API payload compatibility
- FHIR mapping expectations

## 20.4 State machine tests
- allowed transitions
- invalid transition rejection
- timeout transitions
- compensation paths

## 20.5 Saga tests
- happy path
- retry path
- timeout path
- compensation path
- manual intervention path

## 20.6 Safety tests
- invalid measurement rejection
- ambiguous session quarantine
- duplicate event handling
- stale calibration behavior
- failed canonical publication
- replay consistency

---

# 21. Definition of Done for a Service

A service is done for an iteration only if it has:

1. bounded context documented
2. domain model implemented
3. aggregate roots and value objects tested
4. API and/or broker interfaces implemented
5. outbox enabled
6. inbox/idempotency implemented for consumers
7. observability completed
8. state machine documented and tested if applicable
9. failure and retry paths implemented
10. runbook and deployment manifest added
11. audit/provenance hooks added
12. integration/contract tests passing

---

# 22. Risks and Mitigations

## Risk: too many distributed writers of clinical truth
**Mitigation:** restrict canonical FHIR writes to Clinical Interoperability Service.

## Risk: event contract drift
**Mitigation:** strong versioning, contract tests, schema registry if used.

## Risk: unclear bounded contexts
**Mitigation:** maintain bounded context catalog and ADRs.

## Risk: hidden business logic in consumers
**Mitigation:** all broker handlers route through application use cases.

## Risk: losing raw measurement during failure
**Mitigation:** raw persistence before downstream transformations.

## Risk: false confidence from derived analytics
**Mitigation:** carry confidence and provenance on all derived metrics.

## Risk: complex saga failure modes
**Mitigation:** explicit compensation registry, manual intervention flows, observability.

---

# 23. Immediate Next Steps for Codex

Codex should begin in this order:

1. scaffold shared service template
2. scaffold shared outbox/inbox abstractions
3. create bounded context catalog files
4. implement DeviceRegistryService
5. implement MeasurementAcquisitionService
6. implement TreatmentSessionService
7. implement AuditProvenanceService
8. continue with validation and signal conditioning

For each service, Codex should produce:
- folder structure
- domain model
- commands/events
- persistence schema
- APIs
- broker integration
- tests
- README and runbook
- deployment manifests

---

# 24. Recommended Repository Structure

```text
/platform
  /docs
    /architecture
    /bounded-contexts
    /adr
    /events
    /fhir
    /runbooks
  /shared
    /building-blocks
    /observability
    /messaging
    /state-machines
    /workflow
  /services
    /DeviceRegistryService
    /MeasurementAcquisitionService
    /TreatmentSessionService
    /MeasurementValidationService
    /SignalConditioningService
    /RealtimeSurveillanceService
    /ClinicalAnalyticsService
    /ClinicalInteroperabilityService
    /ReportingService
    /AuditProvenanceService
    /TerminologyConformanceService
    /RealtimeDeliveryService
    /QueryReadModelService
    /AdministrationConfigurationService
    /ReplayRecoveryService
    /WorkflowOrchestratorService
```

---

# 25. Final Architecture Position

This platform should be built as:

- **DDD-aligned bounded contexts**
- **Clean Architecture per service**
- **event-driven microservices**
- **outbox/inbox idempotent messaging**
- **state-machine-governed lifecycle control**
- **saga-based orchestration for long-running workflows**
- **strict canonical FHIR governance**
- **safety-first validation at every step**

The implementation order should favor:
1. trust,
2. acquisition,
3. correlation,
4. validation,
5. canonicalization,
6. real-time visibility,
7. reporting,
8. orchestration and recovery maturity.

This sequence minimizes clinical risk while enabling iterative delivery.

---

# 26. Summary for Engineering Execution

The first implementation wave must create the system’s non-negotiable backbone:
- trusted devices
- accepted raw measurements
- active treatment session identity
- audit trail
- validation path

Only after these are stable should the platform expand into:
- signal conditioning
- canonical FHIR publication
- surveillance and dashboards
- clinical analytics
- reporting
- orchestration maturity
- replay and recovery

This ensures the system grows from a safe clinical foundation rather than from UI or analytics-first assumptions.
