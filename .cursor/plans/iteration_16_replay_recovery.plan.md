---
name: Iteration 16 Replay Recovery
overview: "ReplayRecoveryService—ReplayJob + RecoveryPlanExecution aggregates, catalog replay/recovery integration events, EF + outbox, Dialysis.Replay scopes, port 5015."
todos:
  - id: i16-auth-openapi
    content: ReplayRead/ReplayWrite scopes, OpenAPI + BearerSecurityScan + appsettings
    status: completed
  - id: i16-catalog-events
    content: Tier1 ReplayStarted/Completed/Failed + RecoveryPlanExecuted
    status: completed
  - id: i16-service
    content: Domain/Application/Infrastructure/Api + migration + slnx
    status: completed
  - id: i16-tests
    content: Unit + integration OpenAPI + architecture layering
    status: completed
isProject: false
---

# Iteration 16 — Replay & Recovery

Blueprint: [realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md) §8.15, §1810; catalog [integration_event_catalog.md](integration_event_catalog.md) §Replay & Recovery.

## MVP

- **Aggregates:** `ReplayJob` (start/advance checkpoint/pause/resume/cancel/complete/fail), `RecoveryPlanExecution` (execute → `RecoveryPlanExecuted` event).
- **REST:** `api/v1/replay-recovery/replay-jobs/...`, `api/v1/replay-recovery/recovery-plans/execute`.
- **Persistence:** `replay_recovery_dev`, audit table, outbox/inbox.
- **Port:** `5015`.
