#!/usr/bin/env bash
# Run Simulation.GatewayCli from the repository root (no CLI arguments; use env vars).
#
# Usage (repo root):
#   ./scripts/run-simulation-gateway-cli.sh
#
# Common env: SIMULATION_GATEWAY_BASE, SIMULATION_GATEWAY_TENANT (or SIMULATION_TENANT),
#   SIMULATION_SCENARIO_PREFIX or SIMULATION_GATEWAY_INGEST_PREFIX, SIMULATION_GATEWAY_BEARER_TOKEN
# See tools/Simulation.GatewayCli/README.md

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

exec dotnet run --project "$ROOT/tools/Simulation.GatewayCli"
