#!/usr/bin/env bash
# Apply EF migrations for every bounded context that persists to PostgreSQL.
# Host/port: PGHOST/PGPORT (defaults 127.0.0.1:5432), or override with EF_DATABASE_HOST / EF_DATABASE_PORT.
# Requires Postgres reachable (Aspire AppHost or local instance).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

_ef_host="${EF_DATABASE_HOST:-${PGHOST:-127.0.0.1}}"
_ef_port="${EF_DATABASE_PORT:-${PGPORT:-5432}}"
_ef_user="${PGUSER:-postgres}"
_ef_pass="${PGPASSWORD:-postgres}"

connection_string() {
  local database="$1"
  printf 'Host=%s;Port=%s;Database=%s;Username=%s;Password=%s' \
    "$_ef_host" "$_ef_port" "$database" "$_ef_user" "$_ef_pass"
}

migrate() {
  local service="$1"
  local database="$2"
  export ConnectionStrings__Default="$(connection_string "$database")"
  dotnet ef database update \
    --project "$ROOT/platform/services/${service}/${service}.Infrastructure/${service}.Infrastructure.csproj" \
    --startup-project "$ROOT/platform/services/${service}/${service}.Api/${service}.Api.csproj"
}

dotnet tool restore

migrate AdministrationConfiguration administration_configuration_dev
migrate AuditProvenance audit_provenance_dev
migrate ClinicalAnalytics clinical_analytics_dev
migrate ClinicalInteroperability clinical_interoperability_dev
migrate DeviceRegistry device_registry_dev
migrate FinancialInteroperability financial_interoperability_dev
migrate MeasurementAcquisition measurement_acquisition_dev
migrate MeasurementValidation measurement_validation_dev
migrate QueryReadModel query_read_model_dev
migrate RealtimeSurveillance realtime_surveillance_dev
migrate ReplayRecovery replay_recovery_dev
migrate Reporting reporting_dev
migrate SignalConditioning signal_conditioning_dev
migrate TerminologyConformance terminology_conformance_dev
migrate TreatmentSession treatment_session_dev
migrate WorkflowOrchestrator workflow_orchestrator_dev
