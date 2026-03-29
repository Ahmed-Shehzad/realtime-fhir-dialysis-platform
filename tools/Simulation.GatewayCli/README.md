# Simulation.GatewayCli (`simulate-gateway`)

Console app that **always** runs one **full comprehensive ingest** ([`ComprehensiveRelationalIngest.cs`](ComprehensiveRelationalIngest.cs)): every gateway-routed **POST** (admin, device/session graph, parallel measurements and pipelines, publication retry, surveillance lifecycle, workflow branches, financial chain, read-model rebuild and upserts, replay-recovery, audit/provenance, session complete). **Command-line arguments are ignored**; configure with **environment variables** only.

Default gateway URL is `http://localhost:5100` when `SIMULATION_GATEWAY_BASE` is unset.

## Build / run

From the repository root:

```bash
SIMULATION_GATEWAY_TENANT=default ./scripts/run-simulation-gateway-cli.sh
```

Or:

```bash
SIMULATION_GATEWAY_TENANT=default dotnet run --project tools/Simulation.GatewayCli
```

Or execute `tools/Simulation.GatewayCli/bin/Debug/net10.0/simulate-gateway` (same env vars).

## JetBrains Rider

Open **Run → Edit Configurations…** for `Simulation.GatewayCli`. Use profile **`simulate-gateway`** in [`Properties/launchSettings.json`](Properties/launchSettings.json) (`SIMULATION_GATEWAY_TENANT`, `SIMULATION_SCENARIO_PREFIX`) or set **Environment variables** yourself. **Program arguments** are not used.

## Environment variables

| Variable | Purpose |
|----------|---------|
| `SIMULATION_GATEWAY_BASE` | Gateway base URL (default `http://localhost:5100`). |
| `SIMULATION_GATEWAY_TENANT` | `X-Tenant-Id` (alias: `SIMULATION_TENANT`). |
| `SIMULATION_GATEWAY_BEARER_TOKEN` | `Authorization: Bearer` (non-dev / enforced auth). |
| `SIMULATION_GATEWAY_CORRELATION_ID` | Optional `X-Correlation-Id`. |
| `SIMULATION_GATEWAY_API_VERSION` | API path segment (default `1`). |
| `SIMULATION_GATEWAY_TIMEOUT_SECONDS` | HttpClient timeout seconds (default 120). |
| `SIMULATION_GATEWAY_VERBOSE` | Truthy (`1` / `true` / `yes`) — log request URLs to stderr. |
| `SIMULATION_GATEWAY_INGEST_PREFIX` | Prefix for generated resource names (overrides scenario prefix). |
| `SIMULATION_SCENARIO_PREFIX` | Prefix if `SIMULATION_GATEWAY_INGEST_PREFIX` unset; if both unset, default `sim`. |

**Stdout:** one JSON summary (camelCase) at the end — `treatmentSessionId`, `medicalRecordNumber`, `deviceIdentifier`, measurement and report IDs, audit/provenance IDs, etc. (see `ComprehensiveRelationalIngest.Summary`).

## Prerequisites

- **RealtimePlatform.AppHost** (or equivalent): all services behind the gateway, PostgreSQL migrated.
- **Development** often uses JWT bypass on gateway and services; production-shaped runs need a bearer whose scopes cover all policies touched by the ingest (see `BuildingBlocks.Authorization.PlatformAuthorizationPolicies`).

## Stack footprint

The ingest expects **AdministrationConfiguration**, **MeasurementAcquisition**, **MeasurementValidation**, **SignalConditioning**, **ClinicalInteroperability**, **TreatmentSession**, **AuditProvenance**, **RealtimeSurveillance**, **TerminologyConformance**, **WorkflowOrchestrator**, **ClinicalAnalytics**, **Reporting**, **FinancialInteroperability**, **ReplayRecovery**, **QueryReadModel**, and **RealtimeDelivery** to be up unless you change [`ComprehensiveRelationalIngest.cs`](ComprehensiveRelationalIngest.cs) (there are no skip switches in the executable entry point).

With **pdms-web**, [`scripts/run-dev-backend-and-frontend.sh`](../../scripts/run-dev-backend-and-frontend.sh) waits for gateway health, runs this project once (with `SIMULATION_GATEWAY_TENANT` / `SIMULATION_SCENARIO_PREFIX`), then **Vite** (see `SKIP_GATEWAY_WAIT` / `AUTO_SIMULATOR_SCENARIO` in that script).
