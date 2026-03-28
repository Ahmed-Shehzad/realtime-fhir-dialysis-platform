# Platform runbooks

Operational procedures for the realtime FHIR dialysis learning platform. Use together with [`PRE-PRODUCTION-CHECKLIST.md`](../PRE-PRODUCTION-CHECKLIST.md).

| Runbook | Use when |
|--------|----------|
| [replay-drill.md](replay-drill.md) | Verifying deterministic replay and read-model rebuild paths |
| [failover-database-and-cache.md](failover-database-and-cache.md) | PostgreSQL or Redis unavailable or degraded |
| [outbox-backlog.md](outbox-backlog.md) | Integration events not leaving the outbox |
| [audit-evidence-pack.md](audit-evidence-pack.md) | Collecting audit trails for reviews or simulated assessments |
| [load-smoke.md](load-smoke.md) | Lightweight performance smoke before a demo or cut |

Local health sweep (all APIs up): run [`scripts/verify-local-api-health.sh`](../../scripts/verify-local-api-health.sh) from the repository root after starting services.
