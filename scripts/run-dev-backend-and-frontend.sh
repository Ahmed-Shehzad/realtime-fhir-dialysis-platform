#!/usr/bin/env bash
# Run pdms-web (Vite) and optionally Simulation.GatewayCli once — does NOT start API hosts or the gateway.
# Start DeviceRegistry, TreatmentSession, MeasurementAcquisition, QueryReadModel, RealtimeDelivery, and
# ApiGateway separately (Rider compound, other terminals, etc.).
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
#   ./scripts/run-dev-backend-and-frontend.sh -- --tenant default scenario run --prefix demo
#
# Logs: .run/logs/simulator.log when the simulator runs. Ctrl+C stops Vite; cleanup kills a still-running simulator child.

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

start_simulator_background() {
  echo "Running Simulation.GatewayCli (log: .run/logs/simulator.log)..."
  (
    cd "$ROOT"
    dotnet run --project "$SIM_PROJECT" -- "$@" >> "$LOG_DIR/simulator.log" 2>&1
  ) &
  echo $! >> "$PID_FILE"
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

sim_args=()
pass_through=false
for arg in "$@"; do
  if [[ "$pass_through" == true ]]; then
    sim_args+=("$arg")
  elif [[ "$arg" == "--" ]]; then
    pass_through=true
  fi
done

if [[ ${#sim_args[@]} -gt 0 ]]; then
  start_simulator_background "${sim_args[@]}"
elif [[ "${AUTO_SIMULATOR_SCENARIO:-1}" == "1" ]] || [[ "${AUTO_SIMULATOR_SCENARIO:-1}" == "true" ]]; then
  tenant="${SIMULATION_TENANT:-${SIMULATION_GATEWAY_TENANT:-default}}"
  prefix="${SIMULATION_SCENARIO_PREFIX:-devstack}"
  start_simulator_background --tenant "$tenant" scenario run --prefix "$prefix"
else
  echo "AUTO_SIMULATOR_SCENARIO=0 and no args after -- ; not starting Simulation.GatewayCli."
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
