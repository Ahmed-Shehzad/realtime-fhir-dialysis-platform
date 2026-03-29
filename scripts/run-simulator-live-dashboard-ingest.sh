#!/usr/bin/env bash
# Single entry: Simulation.GatewayCli — full comprehensive ingest, then continuous ticks until Ctrl+C:
#   • Vitals: POST measurements (map, heart-rate, spo2) + sessionFeed vitalsByChannel (Vitals trend chart)
#   • Session feed (live): same broadcast (Simulation.VitalsTrend, summary) for the joined session
#   • Patient context (preview): patientDisplayLabel, sessionStateHint, linkedDeviceIdHint on that sessionFeed
#   • Tenant alerts (live): POST delivery/broadcast/alert each tick (alertFeed)
#
# After stdout JSON, copy treatmentSessionId into pdms-web ?sessionId= and match X-Tenant-Id
# (SIMULATION_GATEWAY_TENANT vs VITE_APP_TENANT_ID, or omit both for "default").
#
# Usage (repository root):
#   ./scripts/run-simulator-live-dashboard-ingest.sh
#
# Optional:
#   SIMULATION_GATEWAY_BASE=http://localhost:5100
#   SIMULATION_GATEWAY_TENANT=default
#
# Live vitals / sessionFeed / alert tick loop (simulate-gateway picks one path):
#   • Set SIMULATION_VITALS_STREAM_INTERVAL_MS to a positive integer (ms), e.g. 2500 — takes precedence; or
#   • Leave it unset and this script sets SIMULATION_CONTINUOUS_LIVE_STREAM=1 so the CLI uses a 2000 ms default; or
#   • Set SIMULATION_CONTINUOUS_LIVE_STREAM=0 yourself (and no interval) to run ingest only, no stream.
#
# See tools/Simulation.GatewayCli/README.md

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

GATEWAY_BASE="${SIMULATION_GATEWAY_BASE:-http://localhost:5100}"
GATEWAY_BASE="${GATEWAY_BASE%/}"

if ! curl -sf --connect-timeout 2 --max-time 5 "${GATEWAY_BASE}/health" -o /dev/null; then
  echo "run-simulator-live-dashboard-ingest: GET ${GATEWAY_BASE}/health failed." >&2
  echo "Start RealtimePlatform.ApiGateway (or AppHost) first, then re-run." >&2
  exit 1
fi

export SIMULATION_GATEWAY_TENANT="${SIMULATION_GATEWAY_TENANT:-${SIMULATION_TENANT:-default}}"

# Match Program.cs ReadVitalsStreamIntervalMilliseconds: explicit ms beats continuous flag.
if [[ -n "${SIMULATION_VITALS_STREAM_INTERVAL_MS:-}" ]]; then
  export SIMULATION_VITALS_STREAM_INTERVAL_MS
elif [[ -n "${SIMULATION_CONTINUOUS_LIVE_STREAM:-}" ]]; then
  export SIMULATION_CONTINUOUS_LIVE_STREAM
else
  export SIMULATION_CONTINUOUS_LIVE_STREAM=1
fi

exec dotnet run --project "$ROOT/tools/Simulation.GatewayCli"
