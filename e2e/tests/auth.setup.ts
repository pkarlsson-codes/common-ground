import { expect, test as setup } from '@playwright/test'
import { ensureBaselineIncomeLimits } from './support/api'

const authFile = 'playwright/.auth/user.json'

// Logs in as the e2e user (config/keycloak/realm-commonground-dev.json) through the real
// gateway + Keycloak flow, creating the user's trust on first run, then saves the session
// for every other test. Also enters the trust's baseline income limits.
setup('authenticate', async ({ page }) => {
  // This is the first page load after Playwright starts a fresh Vite server, which transforms all ~500
  // modules on demand. That took 26 s on a busy machine (a game running), past the default 10 s expect.
  setup.setTimeout(120_000)
  // Straight to sign-in: on a demo server / shows the Welcome page to a signed-out visitor.
  await page.goto('/auth/login')

  await page.locator('#username').fill('e2e')
  await page.locator('#password').fill('e2e')
  await page.locator('#kc-login').click()

  // The app lands on / and only then redirects a user without a trust to /onboarding, so wait for
  // whichever page it settles on rather than for the URL.
  const today = page.getByRole('heading', { name: 'Today', level: 1 })
  const createTrust = page.getByRole('link', { name: 'Create a trust' })
  await expect(today.or(createTrust)).toBeVisible({ timeout: 90_000 })

  if (await createTrust.isVisible()) {
    await createTrust.click()
    await page.getByPlaceholder('Example Community Land Trust').fill('E2E Community Land Trust')
    await page.getByRole('button', { name: 'Create trust and continue' }).click()
  }

  await expect(page.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()
  await ensureBaselineIncomeLimits(page)
  await page.context().storageState({ path: authFile })
})
