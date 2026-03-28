---
name: Simulated data console harness
overview: Introduce optional .NET console tool(s) that POST realistic payloads to the API gateway to drive read models, SignalR, and cross-service flows for local debugging with pdms-web.
todos:
  - id: scope-cli
    content: Choose single multi-command CLI (gateway base URL + tenant + optional bearer) vs per-service toys
    status: completed
  - id: scenarios
    content: Define ordered scenarios (device → session → patient → measurements → delivery broadcast)
    status: completed
  - id: implement-harness
    content: Add tools/Simulation.GatewayCli (or samples/*) net10.0 + System.CommandLine + HttpClient
    status: completed
  - id: docs-wireup
    content: Document env vars
    status: completed
isProject: true
---

# Simulated data via console → gateway

## Context

- **No existing console apps** in the solution; integration tests use `WebApplicationFactory`-style hosts instead.
- **Ingestion path for manual simulation**: HTTP to `**RealtimePlatform.ApiGateway`** (e.g. `http://localhost:5100`) so browser, CLI, and services share the same routes, auth, and tenancy as production-shaped debugging.
- **Representative APIs** (non-exhaustive):
  - `POST /api/v1/sessions` — create session (`[SessionsController](../../platform/services/TreatmentSession/TreatmentSession.Api/Controllers/SessionsController.cs)`)
  - `POST /api/v1/sessions/{id}/patient`, `/device`, session start/complete — lifecycle
  - `POST /api/v1/devices` — device registry (`[DevicesController](../../platform/services/DeviceRegistry/DeviceRegistry.Api/Controllers/DevicesController.cs)`)
  - `POST /api/v1/measurements` — acquisition (`[MeasurementsController](../../platform/services/MeasurementAcquisition/MeasurementAcquisition.Api/Controllers/MeasurementsController.cs)`)
  - `POST /api/v1/delivery/broadcast/session` / `alert` — pushes SignalR feed (`[DeliveryBroadcastController](../../platform/services/RealtimeDelivery/RealtimeDelivery.Api/Controllers/DeliveryBroadcastController.cs)`)
  - Read model overview: `GET /api/v1/sessions/{id}/overview` (downstream of projections; may require messaging + other services depending on scenario)

## C5 / local auth

- Send `**X-Tenant-Id`** (aligned with `VITE_APP_TENANT_ID` / gateway tenant middleware).
- Optional `**X-Correlation-Id`** (or let server assign per host rules).
- **Development**: gateway + services `Authentication:JwtBearer:DevelopmentBypass: true` allows scope checks without a real JWT; production-shaped runs use a short-lived bearer from your IdP.
- Map CLI config to the same scope sets as your test user (`SessionsWrite`, `DevicesWrite`, `MeasurementsWrite`, `DeliveryWrite`, etc.).

## Architecture choice


| Approach                                                                                             | Pros                                                            | Cons                                                                                 |
| ---------------------------------------------------------------------------------------------------- | --------------------------------------------------------------- | ------------------------------------------------------------------------------------ |
| **A. One `tools/Simulation.GatewayCli`** with subcommands (`session create`, `broadcast session`, …) | Single dependency graph, one place for gateway URL/tenant/token | Can grow large                                                                       |
| **B. Small consoles per bounded context**                                                            | Isolated                                                        | Duplicated HTTP + config                                                             |
| **C. Extend integration tests as “load scripts”**                                                    | Reuses factories                                                | Not ideal for ad-hoc operator-driven simulation; no standalone `dotnet run` artifact |


**Recommendation:** **A** — one harness targeting **only the gateway** (never direct service ports in normal use).

## Runtime dependencies

- **Docker Postgres** + EF migrations for services you touch (`[docker/README.md](../../docker/README.md)`).
- **Multiple APIs running** (gateway + TreatmentSession + DeviceRegistry + MeasurementAcquisition + RealtimeDelivery + QueryReadModel + …) depending on scenario.
- **MassTransit / outbox**: end-to-end clinical flows assume message bus and outbox relays are configured; local defaults may use a transport that still requires **all relevant consumers** running for projections to update. The harness should document a **minimal** path (e.g. broadcast-only for SignalR UI) vs **full** path (session + measurements + read model).

## Implementation sketch (when approved)

- `tools/Simulation.GatewayCli/Simulation.GatewayCli.csproj` (`net10.0`), `Microsoft.Extensions.Hosting` + `System.CommandLine`.
- Options: `--gateway-base`, `--tenant`, `--token`, `--api-version`.
- JSON payloads mirror API request DTOs (copy shape from controllers or OpenAPI).
- Exit non-zero on non-success HTTP; log response body on error.
- No secrets in source; token from env or file path documented in tool `--help`.

## Files (planned)

- `tools/Simulation.GatewayCli/` — new project + README
- Optional: link from `[docker/README.md](../../docker/README.md)` or `[clients/pdms-web/README.md](../../clients/pdms-web/README.md)` under “Simulating traffic”

## Risks

- **Projection lag / missing data**: read model may not reflect writes until consumers and DB are aligned; CLI should not assume synchronous overview updates without documenting delays.
- **Idempotency**: repeated runs create new sessions/devices unless CLI stores IDs in a local state file (future enhancement).

