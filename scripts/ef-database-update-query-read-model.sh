#!/usr/bin/env bash
# Apply EF migrations for QueryReadModel only (session overview + related read-model tables).
# Host/port: PGHOST/PGPORT or EF_DATABASE_HOST / EF_DATABASE_PORT (same as ef-database-update-all-dev.sh).
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

_ef_host="${EF_DATABASE_HOST:-${PGHOST:-127.0.0.1}}"
_ef_port="${EF_DATABASE_PORT:-${PGPORT:-5432}}"
_ef_user="${PGUSER:-postgres}"
_ef_pass="${PGPASSWORD:-postgres}"

dotnet tool restore

export ConnectionStrings__Default="Host=${_ef_host};Port=${_ef_port};Database=query_read_model_dev;Username=${_ef_user};Password=${_ef_pass}"

dotnet ef database update \
  --project "$ROOT/platform/services/QueryReadModel/QueryReadModel.Infrastructure/QueryReadModel.Infrastructure.csproj" \
  --startup-project "$ROOT/platform/services/QueryReadModel/QueryReadModel.Api/QueryReadModel.Api.csproj"
