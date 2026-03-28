# Integration events — saga mapping

Maps [Integration Event Catalog §3](../../.cursor/plans/integration_event_catalog.md) saga-critical events to intended flow roles. When adding orchestration or consumers, align step names and failure handling with this table; new sagas extend the catalog §3 section first.

## Measurement Processing Saga

| Order | Saga intent (logical) | Catalog integration event | Tier 1 CLR (shared) | Failure / compensation events (catalog §3) |
|------:|------------------------|----------------------------|------------------------|---------------------------------------------|
| 1 | Raw measurement accepted for pipeline | `MeasurementAcceptedIntegrationEvent` | `RealtimePlatform.IntegrationEventCatalog.MeasurementAcceptedIntegrationEvent` | `MeasurementValidationFailedIntegrationEvent`, `WorkflowFailedIntegrationEvent`, `CompensationTriggeredIntegrationEvent` |
| 2 | Binding measurement to session / device context | `MeasurementContextResolvedIntegrationEvent` | *(not Tier 1 — implement when context service exists)* | `MeasurementContextUnresolvedIntegrationEvent`, §3 failure rows |
| 3 | Validation succeeded | `MeasurementValidatedIntegrationEvent` | `RealtimePlatform.IntegrationEventCatalog.MeasurementValidatedIntegrationEvent` | `MeasurementValidationFailedIntegrationEvent`, `MeasurementValidationQuarantinedIntegrationEvent` |
| 4 | Signal processing complete | `SignalConditionedIntegrationEvent` | `RealtimePlatform.IntegrationEventCatalog.SignalConditionedIntegrationEvent` | `SignalDriftDetectedIntegrationEvent`, `SignalDropoutDetectedIntegrationEvent` (operational follow-up; compensation per workflow ADR) |
| 5 | FHIR observation available to subscribers | `CanonicalObservationPublishedIntegrationEvent` | `RealtimePlatform.IntegrationEventCatalog.CanonicalObservationPublishedIntegrationEvent` | `CanonicalObservationPublicationFailedIntegrationEvent`, `CompensationTriggeredIntegrationEvent` |

**Idempotency:** consumers use `eventId` + inbox pattern. **Correlation:** propagate `correlationId`; set `causationId` to the prior step’s `eventId` where the catalog envelope supports it.

## Session lifecycle (Tier 1)

| Catalog integration event | Tier 1 CLR | Role |
|---------------------------|------------|------|
| `DialysisSessionStartedIntegrationEvent` | `RealtimePlatform.IntegrationEventCatalog.DialysisSessionStartedIntegrationEvent` | Session has entered an active treatment phase; emit with constructor `(correlationId, treatmentSessionId, patientId)` so `sessionId` / `patientId` are set on the envelope (not duplicated in `payload`). |

## Session Completion Saga

| Order | Saga intent | Catalog integration event | Notes |
|------:|-------------|---------------------------|--------|
| 1 | Session ended successfully | `DialysisSessionCompletedIntegrationEvent` | Complements Tier 1 `DialysisSessionStartedIntegrationEvent` (session lifecycle). |
| 2 | Analytics finished | `SessionAnalysisCompletedIntegrationEvent` | |
| 3 | Human-readable report ready | `SessionReportGeneratedIntegrationEvent` | |
| 4 | Diagnostic artifact published | `DiagnosticReportPublishedIntegrationEvent` | External exposure per catalog §4. |

Failure handling for long-running completion flows uses §3 **Failure & Compensation** (`WorkflowFailedIntegrationEvent`, `CompensationTriggeredIntegrationEvent`) plus service-specific quarantine events as they are implemented.

## Surveillance (Tier 1)

| Catalog integration event | Tier 1 CLR | Role |
|---------------------------|------------|------|
| `AlertRaisedIntegrationEvent` | `RealtimePlatform.IntegrationEventCatalog.AlertRaisedIntegrationEvent` | Operational and compliance exposure per catalog §4; handlers remain idempotent on `eventId`. |

## Failure & Compensation (cross-saga)

Events from [catalog §3 — Failure & Compensation](../../.cursor/plans/integration_event_catalog.md):

- `MeasurementValidationFailedIntegrationEvent`
- `CanonicalObservationPublicationFailedIntegrationEvent`
- `WorkflowFailedIntegrationEvent`
- `CompensationTriggeredIntegrationEvent`

Plans for new workflows must state which of these are emitted, at which step, and how they surface in audit/operations.

## Related docs

- [Integration event contract](integration-event-contract.md) — envelope fields and serializer.
- [Integration events — plan considerations](../../.cursor/plans/integration_events_plan_considerations.plan.md) — governance and outbox rules.
