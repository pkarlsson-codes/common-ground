import { expect, test } from '@playwright/test'
import { uniqueId } from '../support/ui'

// Journeys (docs/journeys/onboarding/):
//   create-a-trust — a brand-new user is sent to Onboarding, creates a trust, and lands in the app

// A fresh, signed-out browser: the user registers a new Keycloak account, so it belongs to no trust.
// The browser's time zone prefills the trust's Time Zone.
test.use({ storageState: { cookies: [], origins: [] }, timezoneId: 'America/Denver' })

test('a new user without a trust creates one and is taken into the app', async ({ page }) => {
  const username = `e2e-${uniqueId()}`
  await page.goto('/')
  await page.getByRole('link', { name: 'Register' }).click()
  await page.locator('#username').fill(username)
  await page.locator('#email').fill(`${username}@example.com`)
  await page.locator('#firstName').fill('New')
  await page.locator('#lastName').fill('Founder')
  await page.locator('#password').fill('e2e-password')
  await page.locator('#password-confirm').fill('e2e-password')
  await page.getByRole('button', { name: 'Register' }).click()

  // Signed in but not in a trust: every page redirects to Onboarding.
  await expect(page).toHaveURL(/\/onboarding$/)
  await expect(page.getByRole('heading', { name: "You're not part of a trust yet" })).toBeVisible()
  await expect(page.getByText('invite from a trust admin')).toBeVisible()
  await page.goto('/land-parcels')
  await expect(page).toHaveURL(/\/onboarding$/)

  await page.getByRole('link', { name: 'Create a trust' }).click()
  await expect(page).toHaveURL(/\/signup$/)
  await expect(page.getByRole('heading', { name: 'Create your trust' })).toBeVisible()

  const submit = page.getByRole('button', { name: 'Create trust and continue' })
  await page.getByRole('textbox', { name: 'Trust Name' }).fill('   ')
  await submit.click()
  await expect(page.getByText('Please enter a community land trust name.')).toBeVisible()

  const timeZone = page.getByRole('combobox', { name: 'Time Zone' })
  await expect(timeZone).toHaveValue('America/Denver')
  await timeZone.selectOption('America/Chicago')

  await page.getByRole('textbox', { name: 'Trust Name' }).fill(`Founders CLT ${uniqueId()}`)
  await submit.click()
  await expect(page).toHaveURL(/:3000\/$/)

  // Now in a trust, Onboarding and Create Trust send the user into the app.
  await page.goto('/onboarding')
  await expect(page).toHaveURL(/:3000\/$/)
  await page.goto('/signup')
  await expect(page).toHaveURL(/:3000\/$/)
})
