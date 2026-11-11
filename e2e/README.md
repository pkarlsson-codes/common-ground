# End-to-end tests

Playwright tests that drive the Management app through the real stack: Keycloak login → gateway → API → Postgres.

## Running

```sh
npm run e2e:install   # once: installs Playwright and Chromium
npm run infra:start   # docker infra must be up
npm run e2e           # or e2e:ui for the interactive runner, e2e:report for the last HTML report
```

`npm run e2e` runs `run.mjs`, which passes its arguments to `playwright test`. When a run fails, it copies the console
output (`output.txt`), the HTML report and the traces to `failed-runs/<time>/`, so the next run can't overwrite them,
and keeps the 5 newest. Open one with `npx playwright show-report failed-runs/<time>/playwright-report`.

A run uses 4 workers (4 Chromium instances). Playwright's own default, half the CPU cores, ran a 32 GB machine out of
memory. Set `E2E_WORKERS` to change it: `E2E_WORKERS=8 npm run e2e`.

To watch a run as a demo, `npm run e2e:demo` runs headed, one test at a time, pausing 400 ms before each action.
Set `SLOWMO` to change the pause, and pass a path to run one area: `SLOWMO=800 npm run e2e:demo -- tests/purchase-workflows`.

Playwright builds the solution, runs the seed command (`npm run db:seed`), and starts the API (:5000), Management
gateway (:4000), and web app (:3000) itself, then stops them when the run ends. The seed command seeds a fresh database
in full (about 15 s) and skips what is already seeded (about 3 s), so a run right after `npm run db:reset` works. **Stop your own dev servers first** — a run fails fast with
`http://localhost:<port> is already used` rather than testing against a server that may be running stale code.

## Login

`tests/auth.setup.ts` logs in as `e2e` / `e2e` (defined in `config/keycloak/realm-commonground-dev.json`) and, on the
first run against a fresh database, creates the user's trust via onboarding. The session is saved to
`playwright/.auth/user.json` and reused by every test.

The browser runs in `E2E_TIME_ZONE` (`tests/support/timeZone.ts`, America/Denver), so the trust it creates gets that
time zone and `isoDate()` gives the trust's today. A trust created before this was set may be in another zone; run
`npm run db:reset` if dates are off by one.

The realm file is only imported when the realm doesn't exist yet, so on an existing Keycloak either run
`npm run infra:reset`, or create the user with Keycloak's admin CLI.

## Writing tests

- One spec file per journey area under `tests/<area>/`, mirroring `docs/journeys/<area>/`. Name the journeys a spec
  covers in a comment at the top.
- Every test creates its own data with unique names (`uniqueId()`), so tests run in parallel against a database that
  already holds data. Never depend on data from another test or a previous run.
- Use the helpers in `tests/support/ui.ts`. The app's form labels aren't linked to their inputs, so use `field(scope,
  'Label')` instead of `getByLabel`, and `panel(page, 'Title')` to target a slide-over.
