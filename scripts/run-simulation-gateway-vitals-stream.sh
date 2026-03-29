#!/usr/bin/env bash
# Deprecated name — calls run-simulator-live-dashboard-ingest.sh (same behavior).

set -euo pipefail
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
exec "${SCRIPT_DIR}/run-simulator-live-dashboard-ingest.sh" "$@"
