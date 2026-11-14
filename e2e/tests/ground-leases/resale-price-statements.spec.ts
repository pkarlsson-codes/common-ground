import { expect, test, type Page } from '@playwright/test'
import { createGroundLease } from '../support/api'
import { detailValue, expectToast, field, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/prepare-resale-price-statement, view-ground-lease-detail (Resale Price
//   Statement section), docs/journeys/today/work-through-today (Resale price statement due)

const statementSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Resale Price Statement$/ }) })

const todaySection = (page: Page, name: string) => page.getByRole('region', { name: new RegExp(`^${name}`) })

// createGroundLease: appraisal based, 25% share, base price $200,000, original appraised value $250,000, started a
// year ago, so its first statement is due today.

test('staff prepare a statement on a lease, print the letter, and record it sent', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Statement Owner ${uniqueId()}`
  await createGroundLease(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)

  const section = statementSection(page)
  await expect(section).toContainText('No statements sent')
  const dueBefore = await detailValue(section, 'Next Due').textContent()

  await section.getByRole('button', { name: 'Prepare Statement' }).click()
  const prepare = panel(page, 'Resale Price Statement')
  await expect(prepare).toContainText('Appraisal Based, 25% of the rise in market value')
  await expect(prepare.getByRole('button', { name: 'Record Sent' })).toBeDisabled()

  await field(prepare, 'Market Value ($)').fill('320000')
  await field(prepare, 'Value Source').fill('County assessment 2026')
  await expect(prepare).toContainText('$217,500.00')
  await expect(prepare).toContainText(
    'Purchase price $200,000.00 plus 25% of the rise in market value: market value $320,000.00 (County assessment 2026) less $250,000.00 at purchase = $217,500.00',
  )

  const popupPromise = page.waitForEvent('popup')
  await prepare.getByRole('button', { name: 'Print Letter' }).click()
  const letterPage = await popupPromise
  const letter = letterPage.getByRole('article', { name: "Your home's resale price" })
  await expect(letter).toContainText(`Dear ${homeowner},`)
  await expect(letter).toContainText('the most your home can sell for on')
  await expect(letter).toContainText('$217,500.00')
  await expect(letter).toContainText('This letter is for your information and is not an offer.')
  await letterPage.close()

  await field(prepare, 'Delivery').selectOption({ label: 'Certified Mail' })
  await prepare.getByRole('button', { name: 'Record Sent' }).click()
  await expectToast(page, 'Statement recorded')
  await expect(prepare).toBeHidden()

  const row = section.getByRole('table', { name: 'Resale price statements' }).locator('tbody tr')
  await expect(row).toHaveCount(1)
  await expect(row).toContainText('$217,500.00')
  await expect(row).toContainText('Certified Mail')
  await expect(row).toContainText('Market value $320,000.00, County assessment 2026')
  await expect(detailValue(section, 'Next Due')).not.toHaveText(dueBefore ?? '')
})

test('a lease whose statement is due waits on Today until it is recorded there', async ({ page }) => {
  const homeowner = `Statement Today ${uniqueId()}`
  await createGroundLease(page, homeowner)

  // Today reads the lease view, which catches up with the new lease after the POST returns.
  const row = todaySection(page, 'Stewardship').getByRole('row', { name: new RegExp(`Resale price statement due.*${homeowner}`) })
  await expect(async () => {
    await page.goto('/')
    await expect(row).toContainText(`${homeowner}: resale price statement due, none sent yet`, { timeout: 2_000 })
  }).toPass()

  await row.getByRole('button', { name: /^Prepare statement: / }).click()
  const prepare = panel(page, 'Resale Price Statement')
  await field(prepare, 'Market Value ($)').fill('300000')
  await field(prepare, 'Value Source').fill('Broker opinion')
  await expect(prepare).toContainText('$212,500.00')
  await prepare.getByRole('button', { name: 'Record Sent' }).click()
  await expectToast(page, 'Statement recorded')

  await expect(page).toHaveURL(/\/$/)
  await expect(page.getByRole('row', { name: new RegExp(homeowner) })).toHaveCount(0)
})
