---
name: .NET Aspire orchestration
overview: Add Aspire AppHost with Postgres + all platform APIs/gateway; shared ServiceDefaults for optional HTTP resilience/service discovery without duplicating platform OTEL.
todos:
  - id: apphost
    content: RealtimePlatform.AppHost models Postgres + AddProject for gateway and 16 APIs
    status: completed
  - id: servicedefaults
    content: RealtimePlatform.ServiceDefaults (resilience + discovery + /alive); no duplicate Serilog/OTEL
    status: completed
  - id: slnx
    content: Register AppHost and ServiceDefaults in RealtimeFhirDialysisPlatform.slnx
    status: completed
  - id: cpm-apphost
    content: Directory.Build.props exempts AppHost from CPM + Sonar/Ulid global refs; explicit Aspire package versions
    status: completed
isProject: true
---

## Context

- YARP gateway uses fixed `localhost` ports; `AddProject` uses each API’s `launchSettings.json` URLs.
- Aspire resource names must use hyphens; PostgreSQL database names stay `*_dev` via `AddDatabase(resourceName, databaseName)`.
- `WithReference(db, "Default")` injects `ConnectionStrings:Default` for EF services.
- **First-time DBs** from Aspire are empty: apply EF migrations (or `scripts/dev-database-setup.sh` against the Aspire Postgres endpoint) before scenarios.
- Optional: add `ProjectReference` to `RealtimePlatform.ServiceDefaults` and call `builder.AddServiceDefaults()` / `app.MapDefaultEndpoints()` in any API that should use standardized HTTP client resilience + `/alive`.

## Mermaid

```mermaid
flowchart TB
  subgraph Aspire
    AppHost[RealtimePlatform.AppHost]
    PG[(Postgres container)]
  end
  AppHost --> PG
  AppHost --> Gateway[ApiGateway :5100]
  AppHost --> APIs[16 x *.Api :5001-5017]
  APIs --> PG
```
