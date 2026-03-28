# Pre-production checklist (operational acceptance)

Use for a **learning / staged** cut. Not a substitute for organisational go-live gates.

## Regression and build

- [ ] `dotnet build RealtimeFhirDialysisPlatform.slnx -c Release` succeeds locally or on CI for the release commit.
- [ ] `dotnet test RealtimeFhirDialysisPlatform.slnx -c Release` succeeds.
- [ ] CI run retained with **test-results-trx** artifact uploaded (see `.github/workflows/dotnet.yml`).

## Security configuration (C5-oriented)

- [ ] `Authentication:JwtBearer:DevelopmentBypass` is **false** in any shared/staged environment that mimics production.
- [ ] Scopes under `Authorization:Scopes` match Entra (or IdP) app registration for that environment.
- [ ] No secrets committed; connection strings and keys come from configuration or vault.

## Data plane

- [ ] PostgreSQL migrations applied for all services in scope; databases isolated per environment naming.
- [ ] Redis (if enabled) reachable and `InstanceName` prevents collisions across instances.
- [ ] Outbox relay enabled where messages must leave the service (`RealtimePlatform:OutboxRelay`).

## Operations

- [ ] [`scripts/verify-local-api-health.sh`](../scripts/verify-local-api-health.sh) passes when all required APIs are running (local smoke).
- [ ] Runbooks reviewed: [replay drill](runbooks/replay-drill.md), [failover DB/cache](runbooks/failover-database-and-cache.md), [outbox backlog](runbooks/outbox-backlog.md).
- [ ] Drill log entry completed: replay or failover exercise, or explicit waiver with risk note.

## Observability

- [ ] Logs centralised or collectable for the environment; correlation id propagates on at least one critical path test.
- [ ] Health endpoints respond `/health` for each deployed API.

## Sign-off

- Owner: _________________  Date: _________  
- Notes: _______________________________________________
