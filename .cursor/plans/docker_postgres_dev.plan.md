---
name: Docker Postgres for local dev
overview: Run PostgreSQL in Docker with one database per service (matching existing connection strings), document EF migration apply, keep host appsettings on 127.0.0.1:5432 for gateway debugging.
todos:
  - { id: compose-init, content: Add docker-compose.yml + init script for all *\_dev databases, status: completed }
  - { id: migrate-script, content: Add scripts to apply EF migrations (all services + minimal path), status: completed }
  - { id: docker-readme, content: Add docker/README.md runbook (compose, migrate, gateway + pdms-web), status: completed }
isProject: true
---

# Docker PostgreSQL (development)

## Alignment

- Existing `ConnectionStrings:Default` in platform services use `Host=127.0.0.1;Port=5432;Username=postgres;Password=postgres` and per-service `Database=*_dev`.
- Containers publish `5432:5432`; processes on the host (Rider, `dotnet run`, browser via gateway) require no change if Postgres was already expected on localhost.

## Files

- `docker-compose.yml` (repo root)
- `docker/postgres/init/01-create-databases.sh`
- `scripts/ef-database-update-all-dev.sh` — optional one-shot EF migrate for every bounded context
- `scripts/ef-database-update-query-read-model.sh` — minimal path for dashboard read model
- `docker/README.md`

## Risks

- Port 5432 conflict if local Postgres is already installed — stop host Postgres or change compose port and connection strings / env.
- EF `dotnet-ef` tool must be installed; script documents `dotnet tool restore` if tool manifest exists (check).
