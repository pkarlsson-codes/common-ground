import { expect, test as setup } from '@playwright/test'
import { ensureBaselineIncomeLimits } from './support/api'

const authFile = 'playwright/.auth/user.json'

// Logs in as the e2e user (config/keycloak/realm-commonground-dev.json) through the real
// gateway + Keycloak flow, creating the user's trust on first run, then saves the session
// for every other test. Also enters the trust's baseline income limits.
setup('authenticate', async ({ page }) => {
  await page.goto('/')

  await page.locator('#username').fill('e2e')
  await page.locator('#password').fill('e2e')
  await page.locator('#kc-login').click()

  // The app lands on / and only then redirects a user without a trust to /onboarding, so wait for
  // whichever page it settles on rather than for the URL.
  const today = page.getByRole('heading', { name: 'Today', level: 1 })
  const createTrust = page.getByRole('link', { name: 'Create a trust' })
  await expect(today.or(createTrust)).toBeVisible()

  if (await createTrust.isVisible()) {
    await createTrust.click()
    await page.getByPlaceholder('Example Community Land Trust').fill('E2E Community Land Trust')
    await page.getByRole('button', { name: 'Create trust and continue' }).click()
  }

  await expect(page.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()
  await ensureBaselineIncomeLimits(page)
  await page.context().storageState({ path: authFile })
})
