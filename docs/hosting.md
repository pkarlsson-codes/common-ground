# Hosting CommonGround

How to run CommonGround on one server. The same setup works for a CLT running its own copy and for one operator running a shared copy for several CLTs. The app does not bill anyone.

## What you need

- A Linux server with Docker and the Docker Compose plugin. 4 GB RAM and 40 GB disk is a comfortable size. Keycloak is capped at 1.5 GB in `deploy/compose.yml`, and on a test run the API used about 180 MiB, the gateway 90 MiB and Postgres 85 MiB.
- A domain name with an A record (and AAAA, if you use IPv6) pointing at the server.
- Ports 80 and 443 (TCP and UDP) open to the internet. Caddy gets and renews the TLS certificate by itself.
- An SMTP account. Sign-up requires a verified email, and password reset sends email.

## What runs

`deploy/compose.yml` starts five containers:

| Container | Job |
|---|---|
| `caddy` | TLS and the only public entry point. Sends `/auth-server/*` to Keycloak and everything else to the gateway. |
| `postgres` | Events, read models, Wolverine messages, gateway sessions, Data Protection keys and Keycloak users. |
| `keycloak` | Sign-up, sign-in and password reset. Runs under `/auth-server`. |
| `api` | Domain logic and persistence. Not reachable from outside. |
| `gateway` | Serves the web app, handles OIDC login and proxies `/api` to the API. |

Uploaded documents are files in the `documents` Docker volume, mounted at `/app/data/documents` in the API container.

## First install

1. Get the code onto the server, for example `git clone` into `/opt/commonground`.
2. `cd deploy && cp .env.example .env && chmod 600 .env`
3. Fill in every value in `.env`. Make each secret with `openssl rand -base64 48 | tr -d '/+=' | cut -c1-48`. `.env` holds every secret of the install, so keep it private.
   - `DOMAIN`: the host name from the DNS record.
   - `POSTGRES_PASSWORD`, `INTERNAL_JWT_SIGNING_KEY` (at least 32 characters), `GATEWAY_CLIENT_SECRET`, `KEYCLOAK_ADMIN_PASSWORD`.
   - `SMTP_HOST`, `SMTP_PORT`, `SMTP_USER`, `SMTP_PASSWORD`, `SMTP_FROM`, `SMTP_STARTTLS`, `SMTP_SSL`.
4. `docker compose up -d --build`. The first build takes several minutes because the API and gateway images are built on the server.
5. Open `https://DOMAIN`. Sign up, confirm the email, and the first user creates a trust.

The Keycloak admin console is at `https://DOMAIN/auth-server/admin/`. Log in with `KEYCLOAK_ADMIN_USER` and `KEYCLOAK_ADMIN_PASSWORD`, and use realm `commonground`.

The API does not seed data. Do not run the `seed` development command on a server.

## Keycloak reads some settings only once

Keycloak imports the realm file on its first start only. After that, changing these values in `.env` does nothing:

- `SMTP_*`: change them in the admin console under Realm settings > Email.
- `GATEWAY_CLIENT_SECRET` and `DOMAIN` in the realm: change them in the admin console, or reset the install.

To change another realm setting from the command line, use `kcadm.sh` inside the container:

```
docker compose exec -T keycloak sh -c "/opt/keycloak/bin/kcadm.sh config credentials --server http://localhost:8080/auth-server --realm master --user \$KC_BOOTSTRAP_ADMIN_USERNAME --password \$KC_BOOTSTRAP_ADMIN_PASSWORD && /opt/keycloak/bin/kcadm.sh update realms/commonground -s <field>=<value>"
```

Keycloak refuses connections for a short time after a restart, until its health check passes. Keycloak also caches theme files, so restart it after editing `deploy/keycloak/themes/commonground`.

## Upgrade

1. Take a backup (see below).
2. Update the code on the server.
3. `cd deploy && docker compose up -d --build`. Compose rebuilds only the services whose files changed.

The API applies its database changes at startup.

## Deploying from CI

The `deploy` job in `.github/workflows/ci.yml` pipes `git archive HEAD` over SSH on every push to `main`. On the server, the deploy key's forced command is `deploy/receive.sh`. It unpacks the archive, replaces the whole tree in `/opt/commonground` except `deploy/.env`, and runs `docker compose up -d --build --force-recreate`. Because it replaces the tree, a file deleted in git is deleted on the server too. Every container is recreated, because a running one keeps its bind mounts (the Caddyfile, the Keycloak themes) on the deleted folders.

Install it once, as root:

```
install -m 755 /opt/commonground/deploy/receive.sh /usr/local/bin/commonground-deploy
```

In the deploy user's `~/.ssh/authorized_keys`, start the key's line with:

```
command="/usr/local/bin/commonground-deploy",restrict ssh-ed25519 AAAA...
```

The installed copy does not update itself when `deploy/receive.sh` changes in git. Install it again after such a change. The deploy user needs write access to `/opt/commonground` and permission to run `docker`.

## Backup and restore

`deploy/backup.sh` writes three things to `/var/backups/commonground` and deletes copies older than 7 days:

- `db-<stamp>.sql.gz`: `pg_dumpall` of every database.
- `documents-<stamp>.tar.gz`: the documents volume.
- `env-<stamp>`: a copy of `.env`.

Run it nightly from cron:

```
30 3 * * * root /opt/commonground/deploy/backup.sh
```

Set `BACKUP_DIR` or `BACKUP_KEEP_DAYS` in the cron line to change the folder or the retention.

The backups sit on the same disk as the data. Copy `/var/backups/commonground` to another machine or a storage bucket (for example with `rclone`), or a lost server loses the backups too. The database dump contains gateway session tokens in plain text, and `.env` holds every secret, so keep the copies as private as the server.

To restore into a fresh install of the same version:

1. Put the saved `.env` in `deploy/` and start only Postgres: `docker compose up -d postgres`.
2. Load the dump: `gunzip -c db-<stamp>.sql.gz | docker compose exec -T postgres psql -U commonground postgres`
3. Unpack the documents into the volume: `docker run --rm -v commonground_documents:/data -v /var/backups/commonground:/in alpine tar xzf /in/documents-<stamp>.tar.gz -C /data`
4. `docker compose up -d --build`

## Documents in S3 instead of a local volume

The default stores documents on local disk. To use an S3 bucket instead (AWS S3, Backblaze B2, Hetzner Object Storage, Cloudflare R2 or MinIO), set these environment variables on the `api` service in `compose.yml`:

```
Documents__Store: S3
Documents__S3__Endpoint: ...
Documents__S3__AccessKey: ...
Documents__S3__SecretKey: ...
Documents__S3__UseSsl: "true"
Documents__S3__Bucket: ...
```

Use S3 if the API container can lose its disk, if more than one API instance runs without a shared volume, or if you want documents off the server. The backup script then needs no documents step, but the bucket needs its own backup.

## Demo mode

Demo mode is for a public demo server. Each visitor who signs up gets a sample trust, and every trust is deleted `DEMO_SANDBOX_DAYS` days after it is created.

- Do not turn it on for a server that holds a real CLT's records.
- Turn it on only on a fresh database. A trust created before demo mode was on is never deleted.
- Set `DEMO_ENABLED=true` in `.env`.
- Visitors confirm their email, so SMTP must work. With `DEMO_ENABLED=true` and an empty `SMTP_HOST`, nobody can sign up.
- Keycloak accounts are not deleted with the trust. Remove them in the admin console if a visitor asks.
- `DEMO_MAX_SANDBOXES` caps how many sample and empty trusts exist at once. Past it, onboarding says the demo is full.
- Visitors send feedback from the banner. It is kept after their trust is deleted, with their sign-in email if they allowed a reply. Read it with `deploy/feedback.sh` (the last 30 days), `deploy/feedback.sh 7` or `deploy/feedback.sh all`.

## Logs

Logs go to the container console: `docker compose logs -f api`. Seq is a development tool only.

## Not covered

- Load testing. No load test has been run, so there is no tested limit on how many CLTs one server serves.
- The homeowner gateway. It has no web app yet and is not in the compose file.
- Running more than one API instance.
