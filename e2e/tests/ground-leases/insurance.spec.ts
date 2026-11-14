import { expect, test, type Page } from '@playwright/test'
import { createGroundLease } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/set-insurance-requirements, view-ground-lease-detail (Insurance section)

const insuranceSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Insurance$/ }) })

test('staff set the insurance a lease requires and see the change in its history', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Insured Owner ${uniqueId()}`
  await createGroundLease(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)

  const section = insuranceSection(page)
  await expect(detailValue(section, 'Replacement Value')).toHaveText('Not set')
  await expect(detailValue(section, 'Liability per Occurrence')).toHaveText('Not set')
  await expect(section.getByRole('list', { name: 'Insurance requirements history' })).toHaveCount(0)

  await section.getByRole('button', { name: 'Set Requirements' }).click()
  const set = panel(page, 'Insurance Requirements')
  await expect(field(set, 'Replacement Value ($)')).toHaveValue('')
  await expect(field(set, 'Effective From')).toHaveValue(isoDate())
  await field(set, 'Replacement Value ($)').fill('280000')
  await field(set, 'Liability per Occurrence ($)').fill('300000')
  await set.getByRole('button', { name: 'Save Requirements' }).click()
  await expectToast(page, 'Insurance requirements saved')
  await expect(set).toBeHidden()

  await expect(detailValue(section, 'Replacement Value')).toHaveText('$280,000.00')
  await expect(detailValue(section, 'Liability per Occurrence')).toHaveText('$300,000.00')
  const history = section.getByRole('list', { name: 'Insurance requirements history' }).locator('li')
  await expect(history).toHaveCount(2)
  await expect(history.first()).toContainText('replacement value $280,000.00, liability $300,000.00')
  await expect(history.last()).toContainText('replacement value not set, liability not set')

  // Clearing an amount stops checking it; the form starts from the current amounts.
  await section.getByRole('button', { name: 'Set Requirements' }).click()
  const again = panel(page, 'Insurance Requirements')
  await expect(field(again, 'Liability per Occurrence ($)')).toHaveValue('300000')
  await field(again, 'Liability per Occurrence ($)').fill('')
  await again.getByRole('button', { name: 'Save Requirements' }).click()
  await expectToast(page, 'Insurance requirements saved')
  await expect(detailValue(section, 'Liability per Occurrence')).toHaveText('Not set')
})
