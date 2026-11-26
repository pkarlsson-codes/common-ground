#!/bin/sh
# Receives a `git archive` tar on stdin, replaces the code in APP_DIR with it,
# and restarts the stack. CI's deploy key runs this as its SSH forced command.
#
# The old tree is removed first, so a file deleted in git is deleted on the
# server too. Only deploy/.env, which holds the install's secrets, is kept.
set -eu

APP_DIR="${APP_DIR:-/opt/commonground}"

# One deploy at a time.
exec 9>"${LOCK_FILE:-/var/lock/commonground-deploy}"
flock 9

[ -f "$APP_DIR/deploy/.env" ] || { echo "receive: $APP_DIR/deploy/.env is missing" >&2; exit 1; }

STAGE=$(mktemp -d)
trap 'rm -rf "$STAGE"' EXIT

# Unpack into a staging folder first, so a cut-off upload leaves the live tree alone.
tar -x --no-same-owner -C "$STAGE"
[ -f "$STAGE/deploy/compose.yml" ] || { echo "receive: archive has no deploy/compose.yml" >&2; exit 1; }

# Delete everything except deploy/.env, then copy the new tree in.
find "$APP_DIR" -mindepth 1 ! -path "$APP_DIR/deploy" ! -path "$APP_DIR/deploy/.env" -delete
cp -R "$STAGE/." "$APP_DIR/"

cd "$APP_DIR/deploy"
docker compose up -d --build --remove-orphans </dev/null
docker image prune -f
