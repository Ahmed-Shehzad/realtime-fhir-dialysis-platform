---
name: Iteration 10 Query Read Model
overview: "QueryReadModelService—session overview + alert projections, dashboard summary query, rebuild API, Dialysis.ReadModel scopes, rebuild catalog event, EF, port 5009."
todos:
  - id: i10-catalog-blocks
    content: ReadModelProjectionRebuiltIntegrationEvent + ReadModel scopes + appsettings + OpenAPI scan + transformers
    status: completed
  - id: i10-service
    content: QueryReadModel Domain/Application/Infrastructure/Api + slnx + EF migration
    status: completed
  - id: i10-tests
    content: Unit + integration + architecture tests
    status: completed
isProject: false
---

# Iteration 10 — Query Read Model Service

Blueprint §8.13. **MVP:** `SessionOverviewProjection` and `AlertProjection` read-model rows, GET overview/alerts/dashboard, POST projections rebuild (truncate + stub seed + outbox event). Defer event-driven live updates from other services.
