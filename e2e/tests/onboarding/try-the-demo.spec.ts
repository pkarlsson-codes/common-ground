import { expect, test } from '@playwright/test'
import { uniqueId } from '../support/ui'

// Journeys (docs/journeys/onboarding/):
//   try-the-demo — a signed-out visitor on a demo server sees the Welcome page, registers,
//                  starts a sample trust and lands in it
//
// Needs the API in demo mode: E2E_DEMO=1 npm run e2e -- tests/onboarding/try-the-demo.spec.ts

test.skip(!process.env.E2E_DEMO, 'Runs only against a demo-mode API (E2E_DEMO=1).')

// A fresh, signed-out browser: the visitor registers a new Keycloak account, so it belongs to no trust.
test.use({ storageState: { cookies: [], origins: [] } })

test('a new visitor starts a sample trust and is taken into it', async ({ page }) => {
  // Filling a sample trust takes about 15 s on an idle server; longer while other runs share it.
  test.setTimeout(180_000)

  const username = `demo-${uniqueId()}`
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Try CommonGround with a sample trust' })).toBeVisible()
  await expect(page.getByText("Don't enter real personal information.")).toBeVisible()
  await expect(page.getByRole('link', { name: 'Sign in' })).toHaveAttribute('href', '/auth/login')
  // "Try the demo" opens the registration form, not the sign-in form.
  await page.getByRole('link', { name: 'Try the demo' }).click()
  await page.locator('#username').fill(username)
  await page.locator('#email').fill(`${username}@example.com`)
  await page.locator('#firstName').fill('Demo')
  await page.locator('#lastName').fill('Visitor')
  await page.locator('#password').fill('e2e-password')
  await page.locator('#password-confirm').fill('e2e-password')
  await page.getByRole('button', { name: 'Register' }).click()

  await expect(page).toHaveURL(/\/onboarding$/)
  await expect(page.getByRole('link', { name: 'Create an empty trust' })).toHaveAttribute('href', '/signup')
  const started = page.waitForResponse((r) => r.url().endsWith('/api/v1/demo/sandboxes') && r.request().method() === 'POST')
  await page.getByRole('button', { name: 'Try it with sample data' }).click()
  expect((await started).status()).toBe(200)
  await expect(page.getByText('Filling your sample trust with a few years of records…')).toBeVisible()

  // A reload during the wait comes back to the same wait, not to a second sample trust.
  await page.reload()
  await expect(page.getByText('Filling your sample trust with a few years of records…')).toBeVisible()

  await expect(page).toHaveURL(/:3000\/$/, { timeout: 150_000 })
  await expect(page.getByText(/Sample data for trying CommonGround\. Don't enter real personal information\./)).toBeVisible()

  await page.getByRole('button', { name: 'Send feedback' }).click()
  await page.getByLabel('Feedback').fill('The resale price section is clear.')
  await page.getByRole('button', { name: 'Send', exact: true }).click()
  await expect(page.getByText('Thanks, your feedback was sent')).toBeVisible()

  await page.goto('/homes')
  await expect(page.locator('tbody tr').first()).toBeVisible()
})
