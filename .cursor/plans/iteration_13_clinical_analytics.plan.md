---
name: Iteration 13 Clinical Analytics
overview: "ClinicalAnalyticsService—session analysis aggregate, MVP derived metrics + confidence, integration events from catalog, EF + outbox, Dialysis.Analytics scopes, port 5012."
todos:
  - id: i13-auth-openapi
    content: AnalyticsRead/AnalyticsWrite scopes, OpenAPI transformers + BearerSecurityScan + appsettings sources
    status: completed
  - id: i13-catalog-events
    content: Tier1 integration events (DerivedMetric*, SessionTrend, Confidence, SessionAnalysisCompleted)
    status: completed
  - id: i13-service
    content: Domain/Application/Infrastructure/Api + migration + slnx
    status: completed
  - id: i13-tests
    content: Unit + integration OpenAPI + architecture layering
    status: completed
isProject: false
---

# Iteration 13 — Clinical Analytics

Blueprint: [realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md) §8.7, §1788.

## MVP

- **Aggregate:** `SessionAnalysis` + child `DerivedMetricLine` rows; factory `RunMvpAnalysis` emits catalog integration events.
- **Command:** `RunSessionAnalysisCommand` → audit + persist + outbox.
- **Query:** `GetSessionAnalysisById` for read API.
- **REST:** `POST .../sessions/{id}/analyses`, `GET .../analyses/{analysisId}` (`AnalyticsWrite` / `AnalyticsRead`).
- **Persistence:** PostgreSQL `clinical_analytics_dev`, outbox/inbox, EF audit table `clinical_analytics_audit_log`.
- **Port:** `5012`.

## Mermaid

```mermaid
flowchart LR
  API[SessionAnalysisController] --> RunCmd[RunSessionAnalysisCommand]
  RunCmd --> Agg[SessionAnalysis.RunMvpAnalysis]
  Agg --> EVT[Integration events]
  RunCmd --> DB[(PostgreSQL + outbox)]
```
