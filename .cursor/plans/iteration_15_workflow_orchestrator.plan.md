---
name: Iteration 15 Workflow Orchestrator
overview: "WorkflowOrchestratorService—WorkflowInstance aggregate, lifecycle + MVP step advance, catalog workflow/saga integration events, EF + outbox, Dialysis.Workflow scopes, port 5014."
todos:
  - id: i15-auth-openapi
    content: WorkflowRead/WorkflowWrite scopes, OpenAPI + BearerSecurityScan + appsettings
    status: completed
  - id: i15-catalog-events
    content: Tier1 WorkflowStarted/Completed/Failed/Compensation/ManualIntervention/TimeoutElapsed
    status: completed
  - id: i15-service
    content: Domain/Application/Infrastructure/Api + migration + slnx
    status: completed
  - id: i15-tests
    content: Unit + integration OpenAPI + architecture layering
    status: completed
isProject: false
---

# Iteration 15 — Workflow Orchestrator

Blueprint: [realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md) §8.16, §1802; catalog [integration_event_catalog.md](integration_event_catalog.md) §Workflow / Saga.

## MVP

- **Aggregate:** `WorkflowInstance` + `WorkflowLifecycleState`, `WorkflowKind`, session-scoped coordination.
- **Commands:** start, advance step, complete, fail, compensation, manual intervention, timeout signal.
- **REST:** under `api/v1/workflow-orchestrator/workflows` with `WorkflowWrite` / `WorkflowRead`.
- **Persistence:** `workflow_orchestrator_dev`, outbox/inbox, `workflow_orchestrator_audit_log`.
- **Port:** `5014`.
