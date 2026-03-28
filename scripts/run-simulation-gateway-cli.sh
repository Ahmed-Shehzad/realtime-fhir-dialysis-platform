#!/usr/bin/env bash
# Run Simulation.GatewayCli from the repository root (forwards args to simulate-gateway).
#
# Usage (repo root or any path):
#   ./scripts/run-simulation-gateway-cli.sh --help
#   ./scripts/run-simulation-gateway-cli.sh --tenant default scenario run --prefix demo
#
# Env (optional): SIMULATION_GATEWAY_BASE, SIMULATION_GATEWAY_TENANT, SIMULATION_GATEWAY_BEARER_TOKEN
# See tools/Simulation.GatewayCli/README.md

set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

exec dotnet run --project "$ROOT/tools/Simulation.GatewayCli" -- "$@"
