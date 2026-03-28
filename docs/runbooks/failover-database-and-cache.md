# Runbook: Failover — database and cache

## Goal

Restore or work around loss of **PostgreSQL** or **Redis** during local or staged runs.

## PostgreSQL down

1. Confirm failure: API health checks return unhealthy for the `postgres` probe; logs show connection exceptions.
2. **Stop** traffic to dependent services (load balancer or stop pods/instances) if this were production; for local dev, stop APIs to avoid log noise.
3. **Recover** Postgres (restart service, failover replica, or restore backup per your environment).
4. **Verify** connectivity from host: `psql` or equivalent using the same connection string as `ConnectionStrings:Default`.
5. **Restart** platform APIs in dependency order if needed: registries and session services before downstream consumers.
6. Run [`scripts/verify-local-api-health.sh`](../../scripts/verify-local-api-health.sh) against `/health`.

## Redis down (where enabled)

1. Services with `RealtimePlatform:Redis:Enabled` true may degrade caching or SignalR backplane behaviour depending on configuration.
2. Set `Enabled: false` only for isolated local debugging; do not misrepresent production topology in compliance exercises.
3. Restart Redis; confirm connection string and `InstanceName` prefix.
4. Re-run health script; exercise one write path that uses Redis if applicable.

## Evidence

- Time to detect, time to recover, services restarted, and confirmation that `/health` is green for required APIs.
