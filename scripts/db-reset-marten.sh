#!/usr/bin/env bash
# Drops all Marten-managed schemas, wiping all application data, and clears the gateways'
# Redis sessions, which name users and trusts that no longer exist once the data is gone.
set -euo pipefail

POSTGRES_CONTAINER="${POSTGRES_CONTAINER:-commonground_postgres}"
POSTGRES_DB="${POSTGRES_DB:-commonground}"
POSTGRES_USER="${POSTGRES_USER:-postgres}"
POSTGRES_PASSWORD="${POSTGRES_PASSWORD:-postgres}"
REDIS_CONTAINER="${REDIS_CONTAINER:-commonground_redis}"

psql_exec() {
    local sql="$1"
    docker exec -e PGPASSWORD="$POSTGRES_PASSWORD" "$POSTGRES_CONTAINER" \
        psql -w -h localhost -U "$POSTGRES_USER" -d "$POSTGRES_DB" -c "$sql"
}

SCHEMAS=(
    applicant_selection
    commonground_events
    documents
    home_buyers
    home_buyers_seed
    homeownership
    homeownership_seed
    identity
    property_registry
    property_registry_seed
    stewardship
    stewardship_seed
    trust_management_seed
)

echo "[db-reset-marten] Dropping Marten schemas..."
for schema in "${SCHEMAS[@]}"; do
    echo "[db-reset-marten]   DROP SCHEMA IF EXISTS $schema CASCADE"
    psql_exec "DROP SCHEMA IF EXISTS $schema CASCADE;"
done

# public holds Wolverine inbox/outbox tables — drop and recreate so the schema itself remains valid
echo "[db-reset-marten]   DROP SCHEMA IF EXISTS public CASCADE + recreate"
psql_exec "DROP SCHEMA IF EXISTS public CASCADE; CREATE SCHEMA public;"

echo "[db-reset-marten] Clearing gateway sessions from Redis..."
docker exec "$REDIS_CONTAINER" redis-cli FLUSHDB

echo "[db-reset-marten] Done. Restart the API to recreate schemas, then log in again."
