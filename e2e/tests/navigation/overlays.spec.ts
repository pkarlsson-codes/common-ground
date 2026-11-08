import { expect, test } from '@playwright/test'
import { post } from '../support/api'
import { field, panel, uniqueId } from '../support/ui'

// Journey: docs/journeys/navigation/close-an-overlay.md

test('Escape closes only the top panel, and asks before discarding typed input', async ({ page }) => {
  const id = uniqueId()
  const primaryName = `Overlay ${id}`
  await post(page, '/api/v1/households', {
    members: [{ name: primaryName, email: `overlay-${id}@example.com`, phone: '+12025550143', isPrimary: true }],
    requiresAccessibility: false,
    annualIncome: 60000,
  })

  await page.goto('/households')
  await page.getByRole('searchbox', { name: 'Search households' }).fill(id)
  await page.getByRole('row', { name: new RegExp(primaryName) }).click()
  const detail = panel(page, primaryName)
  await detail.locator('.checklist').getByRole('button', { name: 'Record Housing Counseling' }).click()
  const counseling = panel(page, 'Record Housing Counseling')
  await field(counseling, 'Certification Number').fill('CERT-ESC')

  // First Escape asks; the household panel is untouched.
  await page.keyboard.press('Escape')
  const discard = page.getByRole('dialog').filter({ hasText: /^Discard changes\?What you typed/ })
  await expect(discard).toBeVisible()
  await expect(counseling).toBeVisible()

  // Escape on the prompt keeps editing and keeps the typed text.
  await page.keyboard.press('Escape')
  await expect(discard).toHaveCount(0)
  await expect(field(counseling, 'Certification Number')).toHaveValue('CERT-ESC')

  // Discard closes only the counseling panel.
  await counseling.getByRole('button', { name: 'Cancel' }).click()
  await discard.getByRole('button', { name: 'Discard' }).click()
  await expect(counseling).toHaveCount(0)
  await expect(detail).toBeVisible()

  // An untouched panel closes on one Escape; the next closes the household panel.
  await detail.locator('.checklist').getByRole('button', { name: 'Record CLT Orientation' }).click()
  await expect(panel(page, 'Record CLT Orientation')).toBeVisible()
  await page.keyboard.press('Escape')
  await expect(panel(page, 'Record CLT Orientation')).toHaveCount(0)
  await expect(detail).toBeVisible()
  await page.keyboard.press('Escape')
  await expect(detail).toHaveCount(0)
})
