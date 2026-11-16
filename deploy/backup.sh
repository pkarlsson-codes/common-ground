#!/usr/bin/env bash
# Nightly backup of a CommonGround server: every Postgres database (the app, Keycloak, the gateway's
# token store), the documents volume, and deploy/.env. Run from cron; see docs/deployment.html.
#
#   BACKUP_DIR        where to write (default /var/backups/commonground)
#   BACKUP_KEEP_DAYS  how many days of backups to keep (default 7)
set -euo pipefail

cd "$(dirname "$0")"
dest="${BACKUP_DIR:-/var/backups/commonground}"
keep="${BACKUP_KEEP_DAYS:-7}"
stamp="$(date -u +%Y%m%dT%H%M%SZ)"

umask 077
mkdir -p "$dest"

# Write to .part and rename, so a failed run never leaves a file that looks complete.
docker compose exec -T postgres pg_dumpall -U commonground | gzip > "$dest/db-$stamp.sql.gz.part"
if [ "$(gzip -dc "$dest/db-$stamp.sql.gz.part" | wc -c)" -lt 1000 ]; then
  echo "backup: database dump is empty" >&2
  rm -f "$dest/db-$stamp.sql.gz.part"
  exit 1
fi
mv "$dest/db-$stamp.sql.gz.part" "$dest/db-$stamp.sql.gz"

docker run --rm -v commonground_documents:/data:ro -v "$dest":/out alpine \
  tar czf "/out/documents-$stamp.tar.gz.part" -C /data .
mv "$dest/documents-$stamp.tar.gz.part" "$dest/documents-$stamp.tar.gz"

# .env holds the secrets a restore needs (POSTGRES_PASSWORD, Keycloak admin password).
cp .env "$dest/env-$stamp"

find "$dest" -maxdepth 1 -type f -mtime "+$keep" -delete
echo "backup: wrote $stamp to $dest"
