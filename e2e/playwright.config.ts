import { defineConfig, devices } from '@playwright/test'
import { E2E_TIME_ZONE } from './tests/support/timeZone'

const repoRoot = new URL('..', import.meta.url).pathname
const authFile = 'playwright/.auth/user.json'

// Playwright starts the API, gateway, and web app itself. reuseExistingServer is off on purpose:
// a dev server started before your last edit would run stale code, so a run fails fast with
// "http://localhost:<port> is already used" instead. Stop your dev servers before running e2e.
// Docker infra (npm run infra:start) must already be running.

// SLOWMO=<ms> pauses before every browser action, for watching a run as a demo (npm run e2e:demo).
// Slowed runs can't fit the normal per-test timeout, so it's disabled while SLOWMO is set.
const slowMo = Number(process.env.SLOWMO ?? 0)

const waitFor = (url: string) =>
  `until curl -sf ${url} >/dev/null; do sleep 1; done`

export default defineConfig({
  testDir: './tests',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: 0,
  reporter: [['list'], ['html', { open: 'never' }]],
  timeout: slowMo ? 0 : 30_000,
  expect: { timeout: 10_000 },
  use: {
    baseURL: 'http://localhost:3000',
    timezoneId: E2E_TIME_ZONE,
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
    launchOptions: { slowMo },
  },
  projects: [
    { name: 'setup', testMatch: /auth\.setup\.ts/ },
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'], storageState: authFile },
      dependencies: ['setup'],
    },
  ],
  webServer: [
    {
      // Builds the whole solution once so the gateway can start with --no-build and
      // the two `dotnet run`s don't race on shared obj/ folders.
      command: [
        `curl -sf http://localhost:7080/realms/commonground-dev >/dev/null || { echo "Keycloak is not reachable on :7080. Run: npm run infra:start" >&2; exit 1; }`,
        'dotnet build src/CommonGround.slnx',
        // Seeds a fresh database in full; on a seeded one the seeders skip what exists and it takes ~3 s.
        'dotnet run --no-build --project src/CommonGround.API --launch-profile http -- seed',
        'dotnet run --no-build --project src/CommonGround.API --launch-profile http',
      ].join(' && '),
      cwd: repoRoot,
      url: 'http://localhost:5000/health',
      reuseExistingServer: false,
      timeout: 300_000,
      stdout: 'ignore',
      stderr: 'pipe',
    },
    {
      command: [
        waitFor('http://localhost:5000/health'),
        'dotnet run --no-build --project src/CommonGround.Management.Gateway --launch-profile http',
      ].join(' && '),
      cwd: repoRoot,
      url: 'http://localhost:4000/health',
      reuseExistingServer: false,
      timeout: 300_000,
      stdout: 'ignore',
      stderr: 'pipe',
    },
    {
      command: 'npm run dev',
      cwd: `${repoRoot}src/CommonGround.Management.Web`,
      url: 'http://localhost:3000',
      reuseExistingServer: false,
      timeout: 60_000,
      stdout: 'ignore',
      stderr: 'pipe',
    },
  ],
})
