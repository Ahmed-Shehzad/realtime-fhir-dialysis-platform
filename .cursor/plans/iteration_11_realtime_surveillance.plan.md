---
name: Iteration 11 Realtime Surveillance
overview: "RealtimeSurveillanceService—clinical alert aggregate + lifecycle state machine, session risk state, rule-evaluation MVP, acknowledge/escalate/resolve APIs, catalog integration events, EF, port 5010, Surveillance JWT scopes."
todos:
  - id: i11-catalog-events
    content: Add AlertAcknowledged/Escalated/Resolved + SessionRiskStateChanged integration events; extend OpenAPI/authorization scopes
    status: completed
  - id: i11-service
    content: RealtimeSurveillance Domain/Application/Infrastructure/Api + slnx + EF migration
    status: completed
  - id: i11-tests
    content: Aggregate + handler unit tests; OpenAPI integration tests; architecture layering
    status: completed
isProject: false
---

# Iteration 11 — Realtime Clinical Surveillance

Aligned with [realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md) §8.6 and [integration_event_catalog.md](integration_event_catalog.md) Realtime Surveillance section.

## Scope (learning MVP)

- `SurveillanceAlert` aggregate (`AggregateRoot`): states Active → Acknowledged | Escalated | Resolved; invalid transitions throw.
- `SessionRiskSnapshot` aggregate: one row per `SessionId`; `SessionRiskStateChangedIntegrationEvent` on change.
- **Commands:** raise alert, acknowledge, escalate, resolve, update session risk, evaluate stub rule (`MAP_BELOW_65` → raise hypotension alert if metric below threshold).
- **HTTP:** CRUD-style alert endpoints + session risk GET/PATCH.
- **Outbox:** existing `AlertRaisedIntegrationEvent`; add acknowledge/escalate/resolved/risk-changed CLR types in catalog assembly.
- **C5:** JWT `Dialysis.Surveillance.Read` / `Dialysis.Surveillance.Write`; audit on mutations.

## Mermaid (command flow)

```mermaid
sequenceDiagram
  participant API as Surveillance API
  participant App as Application
  participant Dom as SurveillanceAlert
  participant UoW as UnitOfWork
  API->>App: AcknowledgeAlertCommand
  App->>Dom: Acknowledge(userId)
  Dom-->>App: AlertAcknowledgedIntegrationEvent
  App->>UoW: SaveChangesAsync
```

## Files

- New: `platform/services/RealtimeSurveillance/**`
- Edit: `BuildingBlocks` authorization + OpenAPI transformers; `Tier1IntegrationEvents.cs` (or companion file); all `appsettings.json` under platform APIs; `OpenApiBearerSecurityScan.cs`; `ServiceLayeringRulesTests.cs`; `RealtimeFhirDialysisPlatform.slnx`; new test projects.

## Risks

- Rule engine deferred to stub; production would load rule sets from config service (Iteration 17).
