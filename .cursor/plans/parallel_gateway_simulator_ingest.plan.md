---
name: Parallel gateway simulator ingest
overview: Gateway-only simulator ingests relational demo data in dependency-safe phases and parallel waves via ComprehensiveRelationalIngest; scenario run is comprehensive by default; legacy linear path retained as --legacy.
todos:
  - id: inventory-apis
    content: Inventory gateway routes + controllers; define minimal relational ingest graph (ordered prerequisites + parallel sets) per domain (device/session/measurements/read-model/delivery/etc.)
    status: completed
  - id: auth-contract
    content: Document and implement dev auth contract — gateway Ingress policy vs downstream ScopeOrBypass; optional composite dev JWT or --token for staging; env vars
    status: completed
  - id: refactor-cli-structure
    content: Restructure tools/Simulation.GatewayCli (phases, HTTP client factory, path registry); remove dead/duplicate scenario code paths
    status: completed
  - id: parallel-orchestration
    content: Implement phase barrier + Task.WhenAll fan-out with shared RunContext (tenant, correlation, bearer, API version); fail-fast or partial failure policy
    status: completed
  - id: idempotent-consistency
    content: Define stable external IDs (prefix + Ulid), ordering rules (register device before link, create session before start), and retry/idempotency notes for safe re-runs
    status: completed
  - id: scripts-docs
    content: Update scripts/run-dev-backend-and-frontend.sh, DEVELOPMENT-ENVIRONMENT.md, Simulation.GatewayCli README for ingest run, --legacy, --skip-financial, and comprehensive stdout JSON
    status: completed
isProject: true
---

# Plan: Parallel data ingest via API gateway (simulator refresh)

## Status

**Implemented.** Orchestration lives in `[tools/Simulation.GatewayCli/ComprehensiveRelationalIngest.cs](../tools/Simulation.GatewayCli/ComprehensiveRelationalIngest.cs)` (not a separate `Orchestration/` folder). Paths and HTTP helpers are in `[GatewayApiPaths.cs](../tools/Simulation.GatewayCli/GatewayApiPaths.cs)` and `[GatewayHttp.cs](../tools/Simulation.GatewayCli/GatewayHttp.cs)`. Entry points in `[Program.cs](../tools/Simulation.GatewayCli/Program.cs)`:


| Command                      | Behaviour                                                                                                                                                                                                                                                                                                                                                                            |
| ---------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| `**scenario run`** (default) | Full relational ingest (admin threshold + rule-set publish, session graph, 2× parallel measurements, validation/conditioning/canonical per measurement, context resolved, parallel audit/surveillance/terminology/rule/workflow/analytics/reporting, finalize+publish report, financial chain, optional delivery+projections, second audit + provenance). Stdout = one JSON summary. |
| `**ingest run`**             | Same as `**scenario run**` (alias).                                                                                                                                                                                                                                                                                                                                                  |
| `**scenario run --legacy**`  | Previous minimal linear demo (device → session → single measurement → broadcasts → optional projections).                                                                                                                                                                                                                                                                            |
| `**--skip-read-model**`      | Skips delivery broadcast + projection upserts.                                                                                                                                                                                                                                                                                                                                       |
| `**--skip-financial**`       | Skips coverage → eligibility → claim → adjudication → EOB.                                                                                                                                                                                                                                                                                                                           |


**Follow-up:** Refresh `[tools/Simulation.GatewayCli/README.md](../tools/Simulation.GatewayCli/README.md)` and `[docs/DEVELOPMENT-ENVIRONMENT.md](../docs/DEVELOPMENT-ENVIRONMENT.md)` so examples mention `ingest run`, `--legacy`, `--skip-financial`, and the comprehensive stdout shape (todo `scripts-docs`).

## Context

- **Goal:** Ingest **consistent, relational** demo/production-like data across platform APIs **through `RealtimePlatform.ApiGateway` only**, using **parallel HTTP** where dependencies allow.
- **Shape:** The old strictly sequential `**scenario run`** body is `**--legacy` only**; the default path is phase-based (~admin → session graph → parallel measurements → per-measurement pipelines in parallel → parallel cross-service wave → reporting → optional financial → optional read-model → audit/provenance capstone).
- **Auth (critical):**
  - **Gateway ingress:** `[GatewayIngressAuthorizationHandler.cs](platform/gateway/RealtimePlatform.ApiGateway/Authorization/GatewayIngressAuthorizationHandler.cs)` — in Development, `Authentication:JwtBearer:DevelopmentBypass` **true** in `[appsettings.Development.json](platform/gateway/RealtimePlatform.ApiGateway/appsettings.Development.json)` allows proxied traffic **without** `Authorization: Bearer`.
  - **Downstream APIs:** Each service enforces **scope policies** (e.g. `[Authorize(Policy = PlatformAuthorizationPolicies.DevicesWrite)]` on `[DevicesController](platform/services/DeviceRegistry/DeviceRegistry.Api/Controllers/DevicesController.cs)`). `[ScopeOrBypassHandler](BuildingBlocks/Authorization/ScopeOrBypassHandler.cs)` **succeeds without scopes** when `IsDevelopment() && JwtBearer:DevelopmentBypass` (per-service `appsettings.Development.json`).
  - **Non-Development / staging:** Simulator must send `**--token` / `SIMULATION_GATEWAY_BEARER_TOKEN`** with JWT `**scp`/`scope`** covering **all** policies touched by the ingest graph (document required scope union); gateway base `appsettings.json` keeps `DevelopmentBypass: false` — ingress then requires authenticated user.

## Relational consistency (microservices)

- **No cross-DB two-phase commit:** Consistency is **workflow order + stable IDs**, not a single transaction.
- **Prerequisites (sequential “Phase 0”):** Admin threshold profile + rule-set draft + **publish** → device register → session create → assign patient → link device → session start.
- **Parallel waves:** Two measurement ingests in parallel; per-measurement validation / conditioning / canonical in parallel across measurements; context resolved in parallel; then a single parallel fan-out (audit fact, surveillance raise, terminology ×2, rule evaluate, workflow start, analytics, report generate); reporting finalize + publish stay ordered; financial chain ordered; delivery + projections parallel when not skipped.
- **Shared context:** One **implicit context** via `[GlobalOptions](../tools/Simulation.GatewayCli/Program.cs)` + generated `deviceIdentifier`, `treatmentSessionId`, `mrn`; stdout emits **one JSON summary** (`ComprehensiveRelationalIngest.Summary`) for automation (`pdms-web`, scripts).

## Architecture (Mermaid)

```mermaid
sequenceDiagram
  participant Sim as Simulation.GatewayCli
  participant GW as ApiGateway
  participant DR as DeviceRegistry
  participant TS as TreatmentSession
  participant MA as MeasurementAcquisition
  participant RD as RealtimeDelivery
  participant QR as QueryReadModel

  Sim->>GW: Phase0 sequential (admin, device, session, link, start)
  GW->>DR: POST /api/v1/devices
  GW->>TS: POST /api/v1/sessions/...
  par Parallel measurements + pipelines
    Sim->>GW: POST measurements (×2)
    GW->>MA: ingest
    Sim->>GW: validation / conditioning / canonical (per id)
  end
  par Parallel cross-service wave
    Sim->>GW: audit, surveillance, terminology, rules, workflow, analytics, reporting.generate
  end
  Sim->>GW: reporting finalize + publish; financial (optional); delivery + projections (optional)
```



## Implementation outline


| Area       | Action                                                                                                                                                                                                                         |
| ---------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
| **CLI**    | `**ingest run`** and `**scenario run`** call `ComprehensiveRelationalIngest.RunAsync`; `**--legacy**` keeps the old `RunLegacyScenarioAsync` path. Optional: presets (`demo`/`full`) or YAML later.                            |
| **HTTP**   | `[GatewayHttp](../tools/Simulation.GatewayCli/GatewayHttp.cs)`: `PostJsonAsync`, `PostJsonReadAsync<T>`, `ExpectNoContentAsync`, `SendPostWithoutBodyAsync`.                                                                   |
| **Paths**  | `[GatewayApiPaths](../tools/Simulation.GatewayCli/GatewayApiPaths.cs)` maps routed ingest endpoints (admin, measurements, audit, surveillance, terminology, workflow, analytics, reporting, financial, delivery, projections). |
| **Errors** | **Fail-fast** on first HTTP error with step label in stderr; no `--continue-on-error` yet.                                                                                                                                     |
| **Tests**  | Optional: smoke test with `WebApplicationFactory` on gateway only, or integration script against local AppHost.                                                                                                                |


## Files touched (actual)

- `[tools/Simulation.GatewayCli/Program.cs](../tools/Simulation.GatewayCli/Program.cs)` — command dispatch; `RunLegacyScenarioAsync`; `RunComprehensiveIngestCommandAsync`.
- `[tools/Simulation.GatewayCli/ComprehensiveRelationalIngest.cs](../tools/Simulation.GatewayCli/ComprehensiveRelationalIngest.cs)` — phased + parallel orchestration + response DTOs.
- `[tools/Simulation.GatewayCli/GatewayApiPaths.cs](../tools/Simulation.GatewayCli/GatewayApiPaths.cs)` — path map.
- `[tools/Simulation.GatewayCli/GatewayHttp.cs](../tools/Simulation.GatewayCli/GatewayHttp.cs)` — JSON POST/read helpers.

**Optional docs/scripts:** `[tools/Simulation.GatewayCli/README.md](../tools/Simulation.GatewayCli/README.md)`, `[scripts/run-dev-backend-and-frontend.sh](../scripts/run-dev-backend-and-frontend.sh)` (still invokes `scenario run`, which now runs comprehensive by default), `[docs/DEVELOPMENT-ENVIRONMENT.md](../docs/DEVELOPMENT-ENVIRONMENT.md)`.

## Dependencies & risks

- **Risk:** Parallel posts to services that **implicitly depend** on eventual consistency (e.g. read-your-writes across services) may **flap** — mitigate with **short ordered delay** after Phase 0 or **retry with backoff** on 404/409 (not implemented in CLI yet).
- **Risk:** **Gateway route order** — paths must match `[RealtimePlatform.ApiGateway` ReverseProxy routes](platform/gateway/RealtimePlatform.ApiGateway/appsettings.json).
- **C5:** Continue sending `**X-Tenant-Id`** on every request; correlate via `**X-Correlation-Id`** for audit trails.

## Alignment

- Supersedes / extends `[.cursor/plans/simulator_react_realtime_use_case.plan.md](.cursor/plans/simulator_react_realtime_use_case.plan.md)` for **multi-service parallel** ingest.
- **Learn-by-doing:** After documentation catch-up, update WIKI/architecture if you maintain a simulator inventory diagram.

