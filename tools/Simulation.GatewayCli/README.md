# Simulation.GatewayCli (`simulate-gateway`)

Console harness that sends **HTTP POST** requests to **RealtimePlatform.ApiGateway** only (default `http://localhost:5100` when `--gateway` / `SIMULATION_GATEWAY_BASE` unset), with optional **`X-Tenant-Id`**, **`Authorization: Bearer`**, and **`X-Correlation-Id`**.

Use this to drive **devices**, **sessions**, **measurements**, **delivery broadcast** (SignalR feed), or the bundled **`scenario run`** pipeline while debugging **pdms-web** or other clients.

## Build / run

From the repository root:

```bash
./scripts/run-simulation-gateway-cli.sh --help
```

Or:

```bash
dotnet run --project tools/Simulation.GatewayCli -- --help
```

Or build and execute the binary under `tools/Simulation.GatewayCli/bin/Debug/net10.0/simulate-gateway`.

## JetBrains Rider

The project is in the solution under **tools**. Open **Run → Edit Configurations…**, ensure the configuration type is **.NET Project** for `Simulation.GatewayCli`, then:

1. **Launch Profiles**: choose **`scenario-run`**, **`help`**, or **`device-register`** (from [`Properties/launchSettings.json`](Properties/launchSettings.json)), or edit **Program arguments** / **Environment variables** on that configuration for one-off debugging.
2. Arguments are everything after `dotnet run -- …` (e.g. `--tenant default scenario run --prefix demo`).
3. Use **Run** or **Debug** as usual; breakpoints in `Program.cs` / `GatewayHttp.cs` bind to this executable.

With **pdms-web**, you can also use [`scripts/run-dev-backend-and-frontend.sh`](../../scripts/run-dev-backend-and-frontend.sh) after the gateway and APIs are up (waits for `/health`, runs a one-shot `scenario run` by default, then Vite — see that script’s header for `SKIP_GATEWAY_WAIT` / `AUTO_SIMULATOR_SCENARIO`).

## Environment variables

| Variable | Purpose |
|----------|---------|
| `SIMULATION_GATEWAY_BASE` | Gateway URL if `--gateway` omitted (default `http://localhost:5100`). |
| `SIMULATION_GATEWAY_TENANT` | `X-Tenant-Id` if `--tenant` omitted. |
| `SIMULATION_GATEWAY_BEARER_TOKEN` | JWT if `--token` omitted (non-dev environments). |

CLI flags override env vars.

## Prerequisites

- Gateway and all **target** services running for the commands you use (see YARP routes).
- PostgreSQL + EF migrations for services that persist data: run **`RealtimePlatform.AppHost`**, then [`scripts/dev-database-setup.sh`](../../scripts/dev-database-setup.sh) from the repo root (see [`docs/DEVELOPMENT-ENVIRONMENT.md`](../../docs/DEVELOPMENT-ENVIRONMENT.md)).
- **Development** hosts often use JWT bypass; production-shaped runs need a real bearer with scopes (`SessionsWrite`, `DevicesWrite`, `MeasurementsWrite`, `DeliveryWrite`, etc.).

## Minimal vs full stack

- **`broadcast session`** / **`broadcast alert`**: needs gateway + **RealtimeDelivery** (and `DeliveryWrite` policy in real auth).
- **`scenario run`**: device → session → patient → device link → start → measurement → **`delivery/broadcast/session`** → **`delivery/broadcast/alert`** → (unless **`--skip-read-model`**) **`POST …/projections/alerts`** and **`POST …/projections/session-overview`** so **`GET /alerts`**, dashboard summary, and SignalR stay aligned for **pdms-web**.
- **`projection upsert-alert`** / **`projection upsert-session-overview`**: direct read-model writes (`ReadModelWrite` scope when auth is enforced).
- Omit **`--skip-read-model`** only when **QueryReadModel** (:5009 via YARP) is running; otherwise scenario stops at projection steps.
- **Read model** (`GET …/sessions/{id}/overview`): after **`scenario run`** without skip, overview is upserted over HTTP; still verify **`GET`** matches your tenant.

## Commands

Global options (any order **before** the first verb): `--gateway`, `--tenant`, `--token`, `--correlation-id`, `--api-version`.

See `simulate-gateway --help` for the current list.

Example:

```bash
./scripts/run-simulation-gateway-cli.sh --tenant default scenario run --prefix demo
```

Same via `dotnet run`:

```bash
dotnet run --project tools/Simulation.GatewayCli -- --tenant default scenario run --prefix demo
```

SignalR-only check (session id must match a group your browser joined, e.g. dashboard `?sessionId=`):

```bash
dotnet run --project tools/Simulation.GatewayCli -- broadcast session \
  --session-id YOUR_ULID \
  --event-type Simulation.Manual \
  --summary "hello from CLI"
```
