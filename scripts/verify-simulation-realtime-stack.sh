#!/usr/bin/env bash
# Verifies gateway health for the Simulation.GatewayCli + pdms-web realtime demo.
# Does not start processes — run the Aspire AppHost (or your IDE compound); see docs/DEVELOPMENT-ENVIRONMENT.md.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
GATEWAY_BASE="${SIMULATION_GATEWAY_BASE:-http://localhost:5100}"
GATEWAY_BASE="${GATEWAY_BASE%/}"

echo "Checking gateway: ${GATEWAY_BASE}/health"
if ! curl -sfS -o /dev/null "${GATEWAY_BASE}/health"; then
  echo "Gateway health check failed. Start RealtimePlatform.ApiGateway (e.g. :5100) and YARP destinations." >&2
  exit 1
fi

echo "OK — gateway responded. Next:"
echo "  1. Postgres: start platform/RealtimePlatform.AppHost (Aspire) or local PostgreSQL on PGHOST/PGPORT"
echo "  2. ApiGateway + DeviceRegistry, TreatmentSession, MeasurementAcquisition, RealtimeDelivery, QueryReadModel (ports in gateway appsettings ReverseProxy)"
echo "  3. pdms-web: npm run dev (see clients/pdms-web/.env.example — align VITE_APP_TENANT_ID with simulate-gateway --tenant)"
echo "  4. ./scripts/run-simulation-gateway-cli.sh --tenant <tenant> scenario run"
