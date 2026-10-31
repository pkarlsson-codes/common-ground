import { expect, test as setup } from '@playwright/test'

const authFile = 'playwright/.auth/user.json'

// Logs in as the e2e user (config/keycloak/realm-commonground-dev.json) through the real
// gateway + Keycloak flow, creating the user's trust on first run, then saves the session
// for every other test.
setup('authenticate', async ({ page }) => {
  await page.goto('/')

  await page.locator('#username').fill('e2e')
  await page.locator('#password').fill('e2e')
  await page.locator('#kc-login').click()

  await page.waitForURL(/localhost:3000\/(onboarding|households)/)

  if (page.url().includes('/onboarding')) {
    await page.getByRole('link', { name: 'Create a trust' }).click()
    await page.getByPlaceholder('Example Community Land Trust').fill('E2E Community Land Trust')
    await page.getByRole('button', { name: 'Create trust and continue' }).click()
  }

  await expect(page).toHaveURL(/\/households/)
  await page.context().storageState({ path: authFile })
})
