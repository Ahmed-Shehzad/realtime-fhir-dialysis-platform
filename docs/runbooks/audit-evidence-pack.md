# Runbook: Audit evidence pack

## Goal

Assemble **audit and provenance evidence** for a review, drill, or simulated assessment (C5-oriented learning platform).

## Sources (typical)

- **Service-specific audit tables**: e.g. `*_audit_log`, `replay_recovery_audit_log`, `administration_configuration_audit_log` — query by time range and `ResourceType`.
- **AuditProvenance** (`5004`): platform audit facts and provenance links per `AuditProvenance.Api` design.
- **Application logs**: structured logs with correlation id and tenant id (if enabled).
- **CI artefacts**: GitHub Actions **test-results-trx** (see `.github/workflows/dotnet.yml`) for regression evidence.

## Steps

1. Define window: incident or release id, UTC from–to, tenant scope if applicable.
2. Export SQL snapshots or CSVs from audit tables (no secrets in exports).
3. Collect TRX (or pipeline logs) for the same release commit SHA.
4. List integration events published in that window if broker logs or outbox metadata are available.
5. Package into a versioned folder or ticket attachment; note jurisdiction and data residency assumptions per `docs/` transparency docs.

## Privacy

- Strip or tokenise patient identifiers unless operating under an approved test dataset policy.
