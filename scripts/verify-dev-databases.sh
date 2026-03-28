#!/usr/bin/env bash
# Report Postgres *_dev databases: public table count + EF migration row count.
# Uses host psql against PGHOST:PGPORT (defaults 127.0.0.1:5432). Start the Aspire AppHost
# (or local Postgres) first; if Aspire maps a different host port, set PGPORT accordingly.
#
# Usage (repository root):
#   ./scripts/verify-dev-databases.sh
# Exit 1 if any expected *_dev database is missing or has no "__EFMigrationsHistory" rows
# (realtimeplatform_dev is optional unless you use --include-shared-schema).

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

export PGHOST="${PGHOST:-127.0.0.1}"
export PGPORT="${PGPORT:-5432}"
export PGUSER="${PGUSER:-postgres}"
export PGPASSWORD="${PGPASSWORD:-postgres}"

EXPECTED=(
  administration_configuration_dev
  audit_provenance_dev
  clinical_analytics_dev
  clinical_interoperability_dev
  device_registry_dev
  financial_interoperability_dev
  measurement_acquisition_dev
  measurement_validation_dev
  query_read_model_dev
  realtime_surveillance_dev
  replay_recovery_dev
  reporting_dev
  signal_conditioning_dev
  terminology_conformance_dev
  treatment_session_dev
  workflow_orchestrator_dev
)

OPTIONAL_DEV=(realtimeplatform_dev)

if ! command -v psql >/dev/null 2>&1; then
  echo "psql is required on PATH (e.g. brew install libpq)." >&2
  exit 1
fi

run_psql() {
  psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" "$@"
}

failures=0

echo "Expected bounded-context *_dev databases (EF should populate __EFMigrationsHistory):"
echo ""

for db in "${EXPECTED[@]}"; do
  exists="$(run_psql -d postgres -tAc "SELECT count(*)::text FROM pg_database WHERE datname='${db}'" 2>/dev/null || echo 0)"
  if [[ "$exists" != "1" ]]; then
    echo "MISSING database: $db"
    failures=$((failures + 1))
    continue
  fi
  tbl="$(run_psql -d "$db" -tAc "SELECT count(*)::text FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';")"
  mig="$(run_psql -d "$db" -tAc "SELECT count(*)::text FROM \"__EFMigrationsHistory\";" 2>/dev/null || echo 0)"
  if [[ "$mig" == "0" ]] || [[ -z "$mig" ]]; then
    echo "FAIL  $db  public_tables=$tbl  ef_migrations=$mig (no migrations applied?)"
    failures=$((failures + 1))
  else
    echo "OK    $db  public_tables=$tbl  ef_migrations=$mig"
  fi
done

echo ""
echo "Optional (shared schema only with dev-database-setup.sh --include-shared-schema):"
for db in "${OPTIONAL_DEV[@]}"; do
  exists="$(run_psql -d postgres -tAc "SELECT count(*)::text FROM pg_database WHERE datname='${db}'" 2>/dev/null || echo 0)"
  if [[ "$exists" != "1" ]]; then
    echo "  (not created) $db"
    continue
  fi
  tbl="$(run_psql -d "$db" -tAc "SELECT count(*)::text FROM information_schema.tables WHERE table_schema='public' AND table_type='BASE TABLE';")"
  mig="$(run_psql -d "$db" -tAc "SELECT count(*)::text FROM \"__EFMigrationsHistory\";" 2>/dev/null || echo 0)"
  echo "  $db  public_tables=$tbl  ef_migrations=$mig"
done

if [[ "$failures" -gt 0 ]]; then
  echo "" >&2
  echo "Verification failed for $failures database(s). Run ./scripts/dev-database-setup.sh from the repo root." >&2
  exit 1
fi

echo ""
echo "All expected *_dev databases have applied EF migrations."
