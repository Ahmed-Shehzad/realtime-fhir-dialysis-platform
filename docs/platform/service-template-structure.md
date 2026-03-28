# Microservice folder structure (template)

Aligned with `realtime_fhir_dialysis_implementation_plan.md` §10. Each deployable service should follow this layout under `platform/services/<ServiceName>/` (or `services/<ServiceName>/`).

```text
src/
  <Service>.Api/
  <Service>.Application/
  <Service>.Domain/
  <Service>.Infrastructure/
  <Service>.Contracts/
tests/
  <Service>.UnitTests/
  <Service>.IntegrationTests/
  <Service>.ContractTests/
  <Service>.ArchitectureTests/
```

Shared packages from Iteration 1 live under `platform/shared/` (for example `RealtimePlatform.Messaging`, `RealtimePlatform.Outbox`, `RealtimePlatform.Observability`, `RealtimePlatform.Workflow`) and are referenced from `Infrastructure` / `Api` as appropriate—not from `Domain`.

Reference implementations in this repo: `platform/services/DeviceRegistry/` (Iteration 2) and `platform/services/MeasurementAcquisition/` (Iteration 3), each with its own PostgreSQL database and EF migrations under `*.Infrastructure/Persistence/Migrations`.

Persistence for this repository: **PostgreSQL + EF Core + Redis** ([`ADR-0001`](../adr/ADR-0001-persistence-postgresql-redis-efcore.md)).
