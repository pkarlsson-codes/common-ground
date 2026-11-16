# CommonGround

Free, open-source software for community land trusts (CLTs). It tracks the land, the homes, the ground leases and the people, from the waiting list through purchase, resale and ongoing stewardship.

A CLT owns land and leases it to homeowners under a long-term ground lease, which keeps the homes affordable. Most CLTs run on spreadsheets or on expensive software. CommonGround aims to be a free alternative that a small CLT can run itself.

**Status:** pre-1.0. It has not been used by a real CLT yet. Expect changes. You can try it at the [live demo](https://commonground.dotnetexperiments.com).

## What it does

- **Property registry:** land parcels, development projects, buildings and homes.
- **Home buyers:** applications, household income eligibility against income limit schedules, the waiting list, and home selection.
- **Homeownership:** ground leases and lease templates, purchase workflows, mortgages, resales with payoff and price statements, lease fees, improvements and heir transfers.
- **Stewardship:** inspections, notices of default, cure periods and cases.
- **Documents:** versioned files attached to leases and homes.
- **Trusts and team:** one install can host several trusts. Staff are invited to a trust by email.
- **Reports:** funder report with CSV downloads, and a CSV import for an existing portfolio.
- **Demo mode:** each visitor who signs up gets a sample trust that is deleted after a set number of days. A demo runs at <https://commonground.dotnetexperiments.com>. Signing up needs a working email address for the confirmation link.

Terminology follows the Grounded Solutions Network's [CLT Technical Manual](https://groundedsolutions.org). CommonGround is not affiliated with or endorsed by Grounded Solutions Network, and it contains no text from the manual. The glossary in [docs/design/glossary.html](docs/design/glossary.html) lists each term as this software uses it.

## How it is built

- .NET 10 modular monolith, one project group per bounded context (`PropertyRegistry`, `HomeBuyers`, `Homeownership`, `Stewardship`, `Documents`, `TrustManagement`, `Identity`).
- Event sourcing with [Marten](https://martendb.io) on PostgreSQL, messaging with [Wolverine](https://wolverinefx.net).
- Two backend-for-frontend gateways (`Management.Gateway`, `Homeowner.Gateway`) that handle OIDC login, sessions and proxying to the API.
- React, TypeScript, Vite and Tailwind for the Management web app, with an API client generated from the OpenAPI spec.
- Keycloak for sign-up and sign-in. Caddy for TLS in production.
- Tests: xUnit unit and integration tests, Vitest, and Playwright end-to-end tests.

Design notes, the glossary and the architecture diagrams are in [docs/design](docs/design). Per-feature user journeys are in [docs/journeys](docs/journeys).

## Run it locally

You need the .NET 10 SDK, Node.js and Docker with the Compose plugin.

```sh
npm install                  # installs the git hook
npm run infra:start          # Postgres, Keycloak, MinIO, Seq
npm run infra:init           # creates the database schemas
npm run db:reset             # resets the event store and seeds sample data

npm run api:start            # API, http://localhost:4000
npm run management:gateway:start   # gateway, http://localhost:5000
npm run management:web:start       # web app, http://localhost:3000
```

Open http://localhost:3000. The dev Keycloak realm has a user `test` with password `test`. The development database and Keycloak credentials in `docker-compose.yml`, `appsettings.Development.json` and `config/keycloak/` are for local use only. Never use them on a server.

Other commands:

```sh
npm run dotnet:test          # .NET tests
npm run management:web:test  # web unit tests
npm run e2e:install && npm run e2e   # Playwright; see e2e/README.md
```

## Host it

[docs/hosting.md](docs/hosting.md) covers a one-server install with Docker Compose, Caddy and Keycloak: DNS, SMTP, first start, upgrades and backups.

## Contributing

This is a one-person project. Issues are welcome. Open an issue before sending a large pull request. To report a security problem, see [SECURITY.md](SECURITY.md).

## License

Copyright (C) 2026 Peter Karlsson.

CommonGround is licensed under the [GNU Affero General Public License v3.0](LICENSE). If you run a modified copy as a network service, you must offer your users the source of your changes.
