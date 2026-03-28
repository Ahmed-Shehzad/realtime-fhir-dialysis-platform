---
name: Iteration 6 Measurement Validation
overview: "Vertical slice for MeasurementValidation bounded context—versioned profile placeholder, outcomes (pass/flag/fail/quarantine), catalog integration events, C5 API, EF persistence, tests."
todos:
  - id: i6-catalog-blocks
    content: Catalog events + JWT Validation scopes (BuildingBlocks + appsettings + OpenAPI scan)
    status: completed
  - id: i6-service
    content: MeasurementValidation Domain/Application/Infrastructure/Api + slnx + migration
    status: completed
  - id: i6-tests
    content: Unit + Integration + Architecture layering tests
    status: completed
isProject: false
---

# Iteration 6 — Measurement Validation Service

## Context

Blueprint `[realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md)` §8.4 and §1737. Iterations 1–5 delivered.

## MVP scope

- Aggregate `MeasurementValidation` with outcome enum (Passed, Flagged, Failed, Quarantined) and rule profile id / rule version metadata.
- Stub rules engine (numeric sample + profile id)—deterministic outcomes for tests.
- HTTP v1 validate + query latest validation for a measurement id.
- Outbox emits catalog-aligned events (`MeasurementValidatedIntegrationEvent`, failure/quarantine/flag/trigger variants).
- Per-service `security_audit_log` + `FhirAuditRecorder` on mutations; dedicated PostgreSQL DB.

## Deferred

- Full `ValidationRuleSet` persistence and admin API.
- Inbox consumer wired from `MeasurementAcceptedIntegrationEvent` (optional follow-up).
