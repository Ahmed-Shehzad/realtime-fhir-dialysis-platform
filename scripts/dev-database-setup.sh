#!/usr/bin/env bash
# Create dev databases (idempotent), apply EF Core migrations, optional shared SchemaOnly DB.
#
# Prerequisites: PostgreSQL reachable on PGHOST:PGPORT (defaults 127.0.0.1:5432).
# Typical local flow: run the Aspire AppHost (`platform/RealtimePlatform.AppHost`) so Postgres
# is listening, then run this script from the repo root. Host `psql` is required for CREATE DATABASE
# steps (e.g. brew install libpq). When databases already exist (Aspire AddDatabase), CREATE steps are no-ops.
#
# Usage (repo root):
#   ./scripts/dev-database-setup.sh
#   ./scripts/dev-database-setup.sh --read-model-only
#   ./scripts/dev-database-setup.sh --databases-only
#   ./scripts/dev-database-setup.sh --include-shared-schema
#
# Environment: PGHOST PGPORT PGUSER PGPASSWORD (default user/password postgres).

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

MIGRATIONS_MODE="all"
DATABASES_ONLY=0
INCLUDE_SHARED_SCHEMA=0

while [[ $# -gt 0 ]]; do
  case "${1:-}" in
    --databases-only)
      DATABASES_ONLY=1
      shift
      ;;
    --read-model-only)
      MIGRATIONS_MODE="read-model"
      shift
      ;;
    --migrations-all)
      MIGRATIONS_MODE="all"
      shift
      ;;
    --include-shared-schema)
      INCLUDE_SHARED_SCHEMA=1
      shift
      ;;
    -h|--help)
      sed -n '2,18p' "$0"
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      exit 1
      ;;
  esac
done

export PGHOST="${PGHOST:-127.0.0.1}"
export PGPORT="${PGPORT:-5432}"
export PGUSER="${PGUSER:-postgres}"
export PGPASSWORD="${PGPASSWORD:-postgres}"

DEV_DATABASES=(
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
  realtimeplatform_dev
)

postgres_responds() {
  if command -v psql >/dev/null 2>&1; then
    if psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -v ON_ERROR_STOP=1 -c "SELECT 1" >/dev/null 2>&1; then
      return 0
    fi
  fi
  if command -v pg_isready >/dev/null 2>&1; then
    if pg_isready -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -q 2>/dev/null; then
      return 0
    fi
  fi
  return 1
}

wait_for_postgres() {
  local attempts=60
  local i=0
  echo "Waiting for Postgres at ${PGHOST}:${PGPORT}..."
  while [[ $i -lt $attempts ]]; do
    if postgres_responds; then
      echo "Postgres is reachable."
      return 0
    fi
    i=$((i + 1))
    sleep 1
  done
  echo "Timeout waiting for Postgres at ${PGHOST}:${PGPORT}. Start the Aspire AppHost (or local Postgres) and ensure PGHOST/PGPORT match the published endpoint (see Aspire dashboard). Install psql (libpq) for database creation." >&2
  return 1
}

if ! command -v psql >/dev/null 2>&1; then
  echo "psql is required for CREATE DATABASE steps (e.g. brew install libpq && brew link --force libpq)." >&2
  exit 1
fi

wait_for_postgres

ensure_database() {
  local db="$1"
  local exists
  exists="$(psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -tAc "SELECT 1 FROM pg_database WHERE datname='${db}'" 2>/dev/null || true)"
  if [[ "$exists" == "1" ]]; then
    echo "  DB exists: ${db}"
  else
    echo "  CREATE DATABASE ${db}"
    psql -h "$PGHOST" -p "$PGPORT" -U "$PGUSER" -d postgres -v ON_ERROR_STOP=1 -c "CREATE DATABASE \"${db}\";"
  fi
}

echo "Ensuring dev databases exist..."
for d in "${DEV_DATABASES[@]}"; do
  ensure_database "$d"
done

if [[ "$DATABASES_ONLY" -eq 1 ]]; then
  echo "Done (--databases-only, skipped migrations)."
  exit 0
fi

echo "Restoring dotnet tools..."
dotnet tool restore

case "$MIGRATIONS_MODE" in
  read-model)
    echo "Applying QueryReadModel migrations..."
    "$ROOT/scripts/ef-database-update-query-read-model.sh"
    ;;
  all)
    echo "Applying all bounded-context migrations..."
    "$ROOT/scripts/ef-database-update-all-dev.sh"
    ;;
  *)
    echo "Unknown MIGRATIONS_MODE: $MIGRATIONS_MODE" >&2
    exit 1
    ;;
esac

if [[ "$INCLUDE_SHARED_SCHEMA" -eq 1 ]]; then
  echo "Applying RealtimePlatform.Persistence (SchemaOnly / realtimeplatform_dev)..."
  REALTIME_PLATFORM_EF_CONNECTION="Host=${PGHOST};Port=${PGPORT};Database=realtimeplatform_dev;Username=${PGUSER};Password=${PGPASSWORD}" \
    dotnet ef database update \
    --project "$ROOT/platform/shared/RealtimePlatform.Persistence/RealtimePlatform.Persistence.csproj"
fi

echo "dev-database-setup finished."
