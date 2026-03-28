#!/usr/bin/env bash
# Print the host TCP port Docker published for the container's Postgres listener (5432/tcp).
#
# Use when `docker ps` shows e.g. `127.0.0.1:55080->5432/tcp` but appsettings use Port=5432:
# set PGPORT or Rider env ConnectionStrings__Default to that port for host-side dotnet/psql.
#
# Usage (repo root):
#   ./scripts/postgres-docker-host-port.sh              # first running container whose name matches postgres (grep -i)
#   ./scripts/postgres-docker-host-port.sh postgres-egegadrg
#   POSTGRES_DOCKER_CONTAINER=my-pg ./scripts/postgres-docker-host-port.sh
#
# Extras:
#   ./scripts/postgres-docker-host-port.sh --connection-snippet device_registry_dev

set -euo pipefail

SNIPPET_DB=""
while [[ $# -gt 0 ]]; do
  case "${1:-}" in
    --connection-snippet)
      SNIPPET_DB="${2:?database name required}"
      shift 2
      ;;
    -h|--help)
      sed -n '2,18p' "$0"
      exit 0
      ;;
    *)
      break
      ;;
  esac
done

CONTAINER="${POSTGRES_DOCKER_CONTAINER:-${1:-}}"
if [[ -z "${CONTAINER}" ]]; then
  CONTAINER="$(docker ps --format '{{.Names}}' | grep -Ei 'postgres' | head -1 || true)"
fi

if [[ -z "${CONTAINER}" ]]; then
  echo "No container: set POSTGRES_DOCKER_CONTAINER, pass a name/id, or run a container with 'postgres' in its name." >&2
  exit 1
fi

LINE="$(docker port "${CONTAINER}" 5432/tcp 2>/dev/null | head -1 || true)"
if [[ -z "${LINE}" ]]; then
  echo "Container ${CONTAINER} has no publish record for 5432/tcp." >&2
  exit 1
fi

# docker port prints "127.0.0.1:55080" or "0.0.0.0:55080"
PORT="${LINE##*:}"
if [[ -z "${PORT}" || "${PORT}" == "${LINE}" ]]; then
  echo "Could not parse host port from: ${LINE}" >&2
  exit 1
fi

if [[ -n "${SNIPPET_DB}" ]]; then
  echo "Host=127.0.0.1;Port=${PORT};Database=${SNIPPET_DB};Username=postgres;Password=postgres"
else
  echo "${PORT}"
fi
