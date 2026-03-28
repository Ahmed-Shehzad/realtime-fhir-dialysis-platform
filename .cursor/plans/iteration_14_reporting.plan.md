---
name: Iteration 14 Reporting
overview: "ReportingService—SessionReport aggregate with Draft/Finalized/Published lifecycle, evidence sections, catalog integration events, EF + outbox, Dialysis.Reporting scopes, port 5013."
todos:
  - id: i14-auth-openapi
    content: ReportingRead/ReportingWrite scopes, OpenAPI transformers + BearerSecurityScan + appsettings sources
    status: completed
  - id: i14-catalog-events
    content: Tier1 SessionReportGenerated, SessionReportFinalized, DiagnosticReportPublished
    status: completed
  - id: i14-service
    content: Domain/Application/Infrastructure/Api + migration + slnx
    status: completed
  - id: i14-tests
    content: Unit + integration OpenAPI + architecture layering
    status: completed
isProject: false
---

# Iteration 14 — Reporting

Blueprint: [realtime_fhir_dialysis_implementation_plan.md](realtime_fhir_dialysis_implementation_plan.md) §8.9, §1795; catalog [integration_event_catalog.md](integration_event_catalog.md) §Reporting.

## MVP

- **Aggregate:** `SessionReport` + ReportSection + SupportingEvidence; `GenerateMvp`, `FinalizeReport`, `PublishDiagnosticReport` with valid transitions only.
- **Commands:** generate, finalize, publish; **Query:** get by id.
- **REST:** `POST .../reporting/sessions/{id}/reports`, `POST .../reporting/reports/{id}/finalize`, `POST .../reporting/reports/{id}/publication`, `GET .../reporting/reports/{id}`.
- **Persistence:** PostgreSQL `reporting_dev`, outbox/inbox, `reporting_audit_log`.
- **Port:** `5013`.

## Mermaid

```mermaid
flowchart LR
  API[SessionReportController] --> Gen[GenerateSessionReportCommand]
  API --> Fin[FinalizeSessionReportCommand]
  API --> Pub[PublishDiagnosticReportCommand]
  Gen --> Agg[SessionReport.GenerateMvp]
  Fin --> SM[State Draft to Finalized]
  Pub --> SM2[State Finalized to Published]
  Agg --> EVT[SessionReportGenerated]
  SM --> EVT2[SessionReportFinalized]
  SM2 --> EVT3[DiagnosticReportPublished]
```
