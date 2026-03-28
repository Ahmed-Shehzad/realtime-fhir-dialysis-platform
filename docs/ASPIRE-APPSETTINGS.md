# Aspire alignment for `appsettings*.json`

This repo runs under **.NET Aspire** from [`platform/RealtimePlatform.AppHost`](../platform/RealtimePlatform.AppHost/AppHost.cs). JSON configuration must stay consistent with that host.

## Postgres-backed APIs

- **Runtime under AppHost:** For each project that calls `.WithReference(<PostgresDatabaseResource>, "Default")`, Aspire injects **`ConnectionStrings__Default`** into the process environment. That **overrides** `ConnectionStrings:Default` from `appsettings*.json`.
- **Standalone `dotnet run` / EF tools:** When that variable is not set, `appsettings.json` and `appsettings.Development.json` supply `ConnectionStrings:Default`.
- **Contract:**
  - **`Host=127.0.0.1`** — reach Postgres from the dev machine through the **published** Docker port.
  - **`Port`** — must match **`devPostgresPublishedPort`** in `AppHost.cs` (currently **5432**). If Docker maps a different host port, fix the **host port binding** (free 5432 or republish Postgres) rather than committing ephemeral ports into shared JSON.
  - **`Database`** — must match the **`AddDatabase(..., postgresDatabaseName)`** name for that bounded context in `AppHost.cs` (e.g. `device_registry_dev`).
  - **Credentials** — align with AppHost `postgres-user` / `postgres-password` parameters.

Each service’s `appsettings.json` includes an **`Aspire`** object (metadata only; not bound to options) that restates this contract.

## ApiGateway

- **Under AppHost:** `ReverseProxy__Clusters__*` destination addresses are set via environment variables (logical in-mesh hosts and explicit **5001–5017** ports). Base [`appsettings.json`](../platform/gateway/RealtimePlatform.ApiGateway/appsettings.json) matches that shape for non-orchestrated tooling.
- **Standalone Development:** [`appsettings.Development.json`](../platform/gateway/RealtimePlatform.ApiGateway/appsettings.Development.json) uses **`http://localhost:<port>/`** for the same Kestrel ports as AppHost.

## RealtimeDelivery.Api

- **No** `WithReference` to a Postgres database in AppHost; there is **no** `ConnectionStrings:Default` injection for Postgres from that resource. The **`Aspire`** section in its `appsettings.json` notes this.

## When the host port is not 5432

Docker may show `127.0.0.1:55080->5432/tcp`: **inside** the container Postgres is still on **5432**; **on your Mac** you must use **55080** for tools and for **standalone** `dotnet run` connection strings.

**Preferred (matches committed `appsettings`):** free **`127.0.0.1:5432`** on the host (stop the other Postgres or container using it), then rerun **AppHost** so the resource publishes **`5432->5432`**.

**If you cannot free 5432:** do **not** commit ephemeral ports. Manage them locally:

| Mechanism | What to set |
|-----------|-------------|
| **Shell / `dev-database-setup.sh`** | `export PGPORT=55080` (or whatever `docker ps` shows). Script honours **`PGPORT`** (see [`scripts/dev-database-setup.sh`](../scripts/dev-database-setup.sh)). |
| **JetBrains Rider** (single API) | Run configuration → Environment variables → `ConnectionStrings__Default` = full Npgsql string with **`Port=55080`**. |
| **dotnet user-secrets** | Per-project: `dotnet user-secrets set ConnectionStrings:Default "Host=127.0.0.1;Port=55080;..."`. |
| **Discover port** | [`scripts/postgres-docker-host-port.sh`](../scripts/postgres-docker-host-port.sh) prints the published port; `--connection-snippet device_registry_dev` prints a full **Default** string for copy/paste. |

**Under AppHost:** Aspire injects **`ConnectionStrings__Default`** for referenced databases; that string uses the **orchestrator’s** view of Postgres (in-cluster host/port). Standalone runs still need the **published** host port when you are not using the injected variable.

## Reference

- Orchestration: [`docs/DEVELOPMENT-ENVIRONMENT.md`](DEVELOPMENT-ENVIRONMENT.md) § Run the local stack (Aspire).

