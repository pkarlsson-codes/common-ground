#!/usr/bin/env bash
# Creates required database schemas if they don't already exist.
# Safe to run repeatedly — all statements are idempotent.
set -euo pipefail

POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-commonground_postgres}"
POSTGRES_DB="${POSTGRES_DB:-commonground}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}"

psql_exec() {
    local sql="$1"
    docker exec -e PGPASSWORD="$POSTGRES_PASSWORD" "$POSTGRES_CONTAINER" \
        psql -w -h localhost -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "$sql"
}

echo "[init-schemas] Waiting for Postgres to be ready..."
for i in $(seq 1 30); do
    if docker exec -e PGPASSWORD="$POSTGRES_PASSWORD" "$POSTGRES_CONTAINER" \
        pg_isready -h localhost -U "$POSTGRES_USER" -d "$POSTGRES_DB" >/dev/null 2>&1; then
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "[init-schemas] ERROR: Postgres did not become ready in time." >&2
        exit 1
    fi
    sleep 1
done

echo "[init-schemas] Creating schemas..."
psql_exec "CREATE SCHEMA IF NOT EXISTS keycloak;"
echo "[init-schemas] Done."
