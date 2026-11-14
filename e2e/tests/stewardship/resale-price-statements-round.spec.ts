import { expect, test } from '@playwright/test'
import { createGroundLease } from '../support/api'
import { expectToast, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/stewardship/send-resale-price-statements

// createGroundLease: appraisal based, 25% share, base price $200,000, original appraised value $250,000, started a
// year ago, so its first statement is due today.

test('staff prepare the yearly round, print the letters, and record them sent', async ({ page }) => {
  const run = uniqueId()
  const first = `Round First ${run}`
  const second = `Round Second ${run}`
  await createGroundLease(page, first)
  await createGroundLease(page, second)

  await page.goto('/stewardship')
  await page.getByRole('button', { name: 'Resale Prices' }).click()
  const search = page.getByRole('searchbox', { name: 'Search resale prices' })
  await search.fill(run)
  const table = page.getByRole('table', { name: 'Resale price statements' })
  await expect(table.locator('tbody tr')).toHaveCount(2)
  await expect(table.locator('tbody tr').first()).toContainText('None')
  await search.fill('')

  await page.getByRole('button', { name: 'Prepare Statements' }).click()
  const prepare = panel(page, 'Prepare Statements')
  // Other tests' leases may be due too; send only this test's two.
  await prepare.getByRole('checkbox', { name: 'Send all' }).uncheck()
  await prepare.getByRole('checkbox', { name: `Send to ${first}` }).check()
  await prepare.getByRole('checkbox', { name: `Send to ${second}` }).check()
  await expect(prepare.getByRole('button', { name: 'Print 2 Statements' })).toBeDisabled()

  await prepare.getByRole('spinbutton', { name: `Market value for ${first}` }).fill('320000')
  await prepare.getByRole('textbox', { name: `Value source for ${first}` }).fill('County assessment 2026')
  await prepare.getByRole('spinbutton', { name: `Market value for ${second}` }).fill('300000')
  await prepare.getByRole('textbox', { name: `Value source for ${second}` }).fill('Broker opinion')

  const statements = prepare.getByRole('table', { name: 'Statements to send' })
  await expect(statements.getByRole('row', { name: new RegExp(first) })).toContainText('$217,500.00')
  await expect(statements.getByRole('row', { name: new RegExp(second) })).toContainText('$212,500.00')

  const popupPromise = page.waitForEvent('popup')
  await prepare.getByRole('button', { name: 'Print 2 Statements' }).click()
  const letters = await popupPromise
  await expect(letters.getByRole('article')).toHaveCount(2)
  await expect(letters.getByRole('article', { name: `Your home's resale price: ${first}` })).toContainText('$217,500.00')
  await expect(letters.getByRole('article', { name: `Your home's resale price: ${second}` })).toContainText('Broker opinion')
  await letters.close()

  await prepare.getByRole('button', { name: 'Record 2 Statements Sent' }).click()
  await expectToast(page, '2 statements recorded')
  await expect(prepare).toBeHidden()

  await search.fill(run)
  await expect(page.getByText('No matching leases')).toBeVisible()
  await page.getByRole('combobox', { name: 'Resale price filter' }).selectOption('all')
  await expect(table.locator('tbody tr')).toHaveCount(2)
  await expect(table.locator('tbody tr').first()).not.toContainText('None')
})
