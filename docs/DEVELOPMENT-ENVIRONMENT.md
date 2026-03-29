# Development environment

Checklist for running the platform locally: **.NET Aspire AppHost** (orchestrates PostgreSQL + gateway + all APIs), optional **pdms-web**, and tooling.

## 1. Prerequisites

| Tool | Purpose | Notes |
|------|---------|--------|
| **.NET SDK 10** | Build, test, EF migrations, Aspire | Pinned in [global.json](../global.json). |
| **Aspire workload** | Run `RealtimePlatform.AppHost` | Install: `dotnet workload install aspire` (see [.NET Aspire](https://learn.microsoft.com/dotnet/aspire/fundamentals/setup-tooling)). |
| **Docker Engine / Docker Desktop** (optional) | Container runtime for Aspire’s PostgreSQL resource on some machines | Aspire may use a container to host Postgres locally; nothing in this repo is started with `docker compose` or repo Dockerfiles. |
| **Node.js** (LTS or current) | `clients/pdms-web` | [clients/pdms-web/README.md](../clients/pdms-web/README.md). |
| **JetBrains Rider** (or VS + C#) | Run AppHost + dashboard | Open [RealtimeFhirDialysisPlatform.slnx](../RealtimeFhirDialysisPlatform.slnx). |
| **`psql`** (optional) | `./scripts/dev-database-setup.sh`, verification | e.g. `brew install libpq`. |

Verify:

```bash
dotnet --version    # expect 10.x aligned with global.json
node --version
```

## 2. Repository tooling (one time per clone)

From the **repository root**:

```bash
dotnet tool restore
```

## 3. Run the local stack (Aspire)

**Startup project:** `platform/RealtimePlatform.AppHost`.

```bash
dotnet run --project platform/RealtimePlatform.AppHost
```

- Opens the **Aspire dashboard**: resource status, logs, and **PostgreSQL connection details** (host/port for host-side `psql` if a non-default port is published).
- Spins up **PostgreSQL** (one server, one logical database per bounded context) and starts **RealtimePlatform.ApiGateway** plus every `*Api` project referenced in the AppHost, with `ConnectionStrings:Default` injected for each service that references its database.
- **`appsettings*.json` and Aspire:** each service `appsettings` includes an **`Aspire`** metadata block; the contract (injected `ConnectionStrings__Default`, published Postgres port, database names) is summarized in [ASPIRE-APPSETTINGS.md](ASPIRE-APPSETTINGS.md).

**First-time schema:** empty databases need EF migrations. With Postgres listening (defaults for scripts: `PGHOST=127.0.0.1`, `PGPORT=5432` — set `PGPORT` from the dashboard if Aspire maps a different host port), from the repo root:

```bash
./scripts/dev-database-setup.sh
```

That script creates any missing `*_dev` databases idempotently and runs `./scripts/ef-database-update-all-dev.sh`. Optional: `./scripts/dev-database-setup.sh --include-shared-schema` for `realtimeplatform_dev`.

**Running APIs without Aspire:** use each API’s `appsettings.Development.json` (`Host=127.0.0.1` for Postgres). The gateway registers **YARP service discovery** (`Microsoft.Extensions.ServiceDiscovery.Yarp`): base [appsettings.json](../platform/gateway/RealtimePlatform.ApiGateway/appsettings.json) uses logical destinations (`http://device-registry/`, …) matching AppHost resource names; [appsettings.Development.json](../platform/gateway/RealtimePlatform.ApiGateway/appsettings.Development.json) overrides those to `http://localhost:5001/` … `5017` for a standalone **Development** gateway. The AppHost injects environment variables on the gateway process so orchestrated runs keep the logical names (and **`WithHttpEndpoint`** pins **5001–5017** and **5100** to match those localhost ports).

## 4. Build and test

```bash
dotnet build RealtimeFhirDialysisPlatform.slnx -c Release
dotnet test RealtimeFhirDialysisPlatform.slnx -c Release --no-build
```

Integration tests that use Testcontainers still require a Docker-compatible runtime when those tests run.

## 5. JetBrains Rider

1. **File → Open** → [RealtimeFhirDialysisPlatform.slnx](../RealtimeFhirDialysisPlatform.slnx).
2. Run **`RealtimePlatform.AppHost`** as the main development entry point (dashboard + all resources).
3. To debug a single API only: run that project with `ASPNETCORE_ENVIRONMENT=Development` and Postgres reachable at the same host/port as your connection string.
4. **RealtimePlatform.ApiGateway** is pinned to **5100** in the AppHost; **`appsettings.Development.json`** adds CORS, JWT **DevelopmentBypass**, debug logging, and **localhost YARP cluster addresses** when you run the gateway outside Aspire. The AppHost **`WithReference`s** every API on the gateway and sets reverse-proxy target env vars so **logical service host names** work under orchestration.
5. **MassTransit**: with an empty Azure Service Bus connection string, each process uses an **in-memory bus** — cross-API messaging needs a shared broker or Aspire hosting all producers/consumers together. See [RealtimePlatform.MassTransit](../platform/shared/RealtimePlatform.MassTransit/DependencyInjection/MassTransitServiceCollectionExtensions.cs).

## 6. Web client (optional)

```bash
cd clients/pdms-web
cp .env.example .env.local
# Set VITE_API_BASE_URL to the gateway URL shown in the Aspire dashboard (or http://localhost:5100 if you run the gateway standalone)
npm install
npm run dev
```

Details: [clients/pdms-web/README.md](../clients/pdms-web/README.md).

**Simulator + Vite:** [`scripts/run-dev-backend-and-frontend.sh`](../scripts/run-dev-backend-and-frontend.sh) does not start the Aspire host — start the AppHost first (or your own terminals). The script waits for `GET /health` on `SIMULATION_GATEWAY_BASE`, then runs **Simulation.GatewayCli** once (**comprehensive** ingest via env: **`SIMULATION_GATEWAY_TENANT`**, **`SIMULATION_SCENARIO_PREFIX`**, …). Set **`AUTO_SIMULATOR_SCENARIO=0`** to skip the simulator and only start Vite. Details: [tools/Simulation.GatewayCli/README.md](../tools/Simulation.GatewayCli/README.md).

## 7. Simulated traffic (CLI)

```bash
SIMULATION_GATEWAY_TENANT=default dotnet run --project tools/Simulation.GatewayCli
```

See [tools/Simulation.GatewayCli/README.md](../tools/Simulation.GatewayCli/README.md) for environment variables and the **stdout JSON** summary (`treatmentSessionId`, `medicalRecordNumber`, measurement and report IDs, etc.).

### 7.1 Simulator + pdms-web realtime (quick checklist)

| Step | What |
|------|------|
| Data | Aspire AppHost + `./scripts/dev-database-setup.sh` so `*_dev` schemas exist. |
| Gateway | Reachable URL (dashboard); **Simulation.GatewayCli** needs **all** API resources the AppHost starts for the full ingest (admin, validation, conditioning, interoperability, audit, surveillance, terminology, workflow, analytics, reporting, financial, replay, delivery, read model, …). |
| Web | `clients/pdms-web`: `.env.local` with **`VITE_APP_TENANT_ID`** = **`SIMULATION_GATEWAY_TENANT`** (same tenant as the simulator); include `readmodel.read` and `delivery.read` in **`VITE_APP_ROLES`**. |
| Verify | `./scripts/verify-simulation-realtime-stack.sh`. |
| Scenario | `./scripts/run-simulation-gateway-cli.sh` (set **`SIMULATION_GATEWAY_TENANT`**) — copy stdout **`treatmentSessionId`** into dashboard **`?sessionId=`**. |

## 8. Common issues

| Symptom | What to check |
|---------|----------------|
| Postgres port conflict | Another process on **5432**; stop it or change Aspire’s published mapping (dashboard shows effective endpoint). |
| EF failures from scripts | `PGHOST` / `PGPORT` must match the Aspire-published Postgres endpoint when not using default **127.0.0.1:5432**. |
| Services unhealthy in dashboard | Logs per resource; ensure migrations ran (`./scripts/verify-dev-databases.sh`). |
| Gateway 502 | Target API not listening on the port in gateway **ReverseProxy:Clusters** (match Aspire-assigned URLs or local launch profiles). |
| `host.docker.internal` / IPv6 “Network unreachable” / connection closed | **`Host=host.docker.internal`** is for processes **inside containers** reaching Postgres on the **host**. If APIs run **on the host** (Rider, `dotnet run`, AppHost children with dev env), use **`Host=127.0.0.1`** (as in each API’s `appsettings.Development.json`) and ensure Postgres listens on `127.0.0.1:5432`. Remove any **`ConnectionStrings__Default`** (or User Secrets) override that sets `host.docker.internal`. Under **Aspire**, prefer the dashboard **Postgres** endpoint / injected connection string instead of hand-rolling `host.docker.internal`. |
| pgAdmin: “no tables” / empty database | In Aspire, each service uses its **own database** (e.g. `device_registry_dev`, `measurement_acquisition_dev`, …). Connect with the server’s host/port (or `host.docker.internal` + published port from dockerized pgAdmin), then in the tree open **Databases →** that name — not only **`postgres`**. Optional: [`scripts/dev-database-setup.sh`](../scripts/dev-database-setup.sh) with **`PGHOST`**, **`PGPORT`**, **`PGPASSWORD`** from the Aspire dashboard ensures DBs exist and runs **`ef-database-update-all-dev.sh`**. With **`ASPNETCORE_ENVIRONMENT=Development`**, each API uses **`IRelationalDatabaseCreator`** to **create the database catalog if missing**, then **`Database.MigrateAsync`** — check logs for *Creating database catalog* / *Applying N pending EF Core migration(s)* or connection errors. |

## Related docs

- [clients/pdms-web/README.md](../clients/pdms-web/README.md) — env vars, CSP, scopes.
