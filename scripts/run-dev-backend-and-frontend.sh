#!/usr/bin/env bash
# Run pdms-web (Vite) and optionally Simulation.GatewayCli once — does NOT start API hosts or the gateway.
# Default simulator: full gateway ingest via dotnet run (env: SIMULATION_GATEWAY_TENANT, SIMULATION_SCENARIO_PREFIX, etc.)
# That is the COMPREHENSIVE gateway ingest (many services). Start RealtimePlatform.AppHost (or full equivalent) first.
#
# Prerequisites: PostgreSQL + APIs + gateway reachable at SIMULATION_GATEWAY_BASE (default http://localhost:5100),
# .NET 10, Node/npm.
#
# Usage:
#   ./scripts/run-dev-backend-and-frontend.sh
#   FRONTEND_ONLY=1 ./scripts/run-dev-backend-and-frontend.sh
#   SKIP_GATEWAY_WAIT=1 ./scripts/run-dev-backend-and-frontend.sh
#   AUTO_SIMULATOR_SCENARIO=0 ./scripts/run-dev-backend-and-frontend.sh   # no scenario; only Vite
#   SIMULATION_TENANT=mytenant ./scripts/run-dev-backend-and-frontend.sh
#   SIMULATION_GATEWAY_TENANT=default SIMULATION_SCENARIO_PREFIX=demo ./scripts/run-dev-backend-and-frontend.sh
#
# Logs: .run/logs/simulator.log when the simulator runs. stdout summary JSON is in that log. Ctrl+C stops Vite; cleanup kills a still-running simulator child.

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

RUN_DIR="$ROOT/.run"
LOG_DIR="$RUN_DIR/logs"
PID_FILE="$RUN_DIR/backend.pids"
GATEWAY_URL="${SIMULATION_GATEWAY_BASE:-http://localhost:5100}"
GATEWAY_URL="${GATEWAY_URL%/}"
SIM_PROJECT="$ROOT/tools/Simulation.GatewayCli/Simulation.GatewayCli.csproj"

mkdir -p "$LOG_DIR"
: > "$PID_FILE"

cleanup() {
  if [[ -f "$PID_FILE" ]]; then
    while read -r pid; do
      [[ -n "$pid" ]] || continue
      kill "$pid" 2>/dev/null || true
    done < "$PID_FILE"
    rm -f "$PID_FILE"
  fi
}

trap cleanup EXIT INT TERM

wait_for_gateway() {
  local maxAttempts="${WAIT_FOR_GATEWAY_ATTEMPTS:-90}"
  local i
  for ((i = 1; i <= maxAttempts; i++)); do
    if curl -sfS -o /dev/null "$GATEWAY_URL/health"; then
      echo "Gateway OK: $GATEWAY_URL/health"
      return 0
    fi
    sleep 1
  done
  echo "Timed out waiting for $GATEWAY_URL/health — start ApiGateway and dependencies first." >&2
  exit 1
}

if [[ "${FRONTEND_ONLY:-}" == "1" ]] || [[ "${FRONTEND_ONLY:-}" == "true" ]]; then
  trap - EXIT INT TERM
  cd "$ROOT/clients/pdms-web"
  exec npm run dev
fi

if [[ ! -d "$ROOT/clients/pdms-web/node_modules" ]]; then
  echo "Installing pdms-web dependencies..."
  (cd "$ROOT/clients/pdms-web" && npm install)
fi

if [[ "${SKIP_GATEWAY_WAIT:-}" != "1" ]] && [[ "${SKIP_GATEWAY_WAIT:-}" != "true" ]]; then
  echo "Waiting for gateway at $GATEWAY_URL ..."
  wait_for_gateway
else
  echo "SKIP_GATEWAY_WAIT=1 — not checking gateway health."
fi

if [[ "${AUTO_SIMULATOR_SCENARIO:-1}" == "1" ]] || [[ "${AUTO_SIMULATOR_SCENARIO:-1}" == "true" ]]; then
  echo "Running Simulation.GatewayCli (log: .run/logs/simulator.log)..."
  tenant="${SIMULATION_TENANT:-${SIMULATION_GATEWAY_TENANT:-default}}"
  prefix="${SIMULATION_SCENARIO_PREFIX:-devstack}"
  (
    cd "$ROOT"
    export SIMULATION_GATEWAY_TENANT="$tenant"
    export SIMULATION_SCENARIO_PREFIX="$prefix"
    dotnet run --project "$SIM_PROJECT" >> "$LOG_DIR/simulator.log" 2>&1
  ) &
  echo $! >> "$PID_FILE"
else
  echo "AUTO_SIMULATOR_SCENARIO=0 — not starting Simulation.GatewayCli."
fi

trap - EXIT
on_frontend_exit() {
  cleanup
  trap - INT TERM EXIT
}
trap on_frontend_exit INT TERM

cd "$ROOT/clients/pdms-web"
echo "Starting Vite: http://localhost:5173 (APIs and gateway must be running separately)"
exec npm run dev
