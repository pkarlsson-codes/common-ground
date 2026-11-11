import { expect, test, type Page } from '@playwright/test'
import { createGroundLeaseTemplate } from '../support/api'
import { expectToast, uniqueId } from '../support/ui'

// Journeys: docs/journeys/onboarding/import-portfolio

const HEADER =
  'Parcel Number,Parcel Address,Parcel Acres,Acquired On,Acquired By,Unit,Home Type,Bedrooms,Bathrooms,Accessible,' +
  'Square Feet,Year Built,Assessed Value,Assessed On,Homeowner Name,Homeowner Email,Lease Template,Lease Start,' +
  'Base Price,Appraised Value at Purchase'

/** Two homes on one parcel: unit A leased to `homeowner`, unit B without a lease. */
const portfolio = (tag: string, template: string, homeowner: string, bedroomsA = '3') =>
  `${HEADER}\n` +
  `APN-${tag},1 Elm St,0.2,3/15/2004,Donation,A,Duplex,${bedroomsA},1.5,no,"1,100",1998,"$180,000",2026-01-01,` +
  `${homeowner},owner-${tag}@example.com,${template},6/1/2005,120000,150000\n` +
  `APN-${tag},1 Elm St,0.2,3/15/2004,Donation,B,Duplex,2,1,yes,900,1998,160000,2026-01-01,,,,,,\n`

async function chooseFile(page: Page, csv: string) {
  await page.getByLabel('Import file').setInputFiles({ name: 'portfolio.csv', mimeType: 'text/csv', buffer: Buffer.from(csv) })
}

test('the page lists the columns and offers a template file', async ({ page }) => {
  await page.goto('/portfolio-import')

  await expect(page.getByRole('heading', { name: 'Import Portfolio' })).toBeVisible()
  await expect(page.getByRole('table', { name: 'Columns' })).toContainText('Appraised Value at Purchase')

  const template = await page.request.get('/portfolio-import-template.csv')
  expect(template.ok()).toBe(true)
  const csv = await template.text()
  expect(csv).toMatch(/^Parcel Number,Parcel Address,/)

  // The example row is valid except for its template name, which no trust has.
  await chooseFile(page, csv)
  await page.getByRole('button', { name: 'Check File' }).click()
  const errors = page.getByRole('table', { name: 'Errors' }).locator('tbody tr')
  await expect(errors).toHaveCount(1)
  await expect(errors).toContainText('Lease Template')
})

test('a file with an error lists it and imports nothing; the fixed file imports, and importing it again skips everything', async ({ page }) => {
  await page.goto('/portfolio-import')
  const { templateName } = await createGroundLeaseTemplate(page)
  const tag = uniqueId()
  const homeowner = `Import Owner ${tag}`

  await chooseFile(page, portfolio(tag, templateName, homeowner, 'three'))
  await page.getByRole('button', { name: 'Check File' }).click()
  const errors = page.getByRole('table', { name: 'Errors' })
  await expect(errors).toContainText('Bedrooms')
  await expect(errors).toContainText('"three" is not a whole number.')
  await expect(page.getByRole('button', { name: 'Import', exact: true })).toHaveCount(0)

  await chooseFile(page, portfolio(tag, templateName, homeowner))
  const summary = page.getByLabel('Import summary')
  // The new template reaches the import's template list through a projection.
  await expect(async () => {
    await page.getByRole('button', { name: 'Check File' }).click()
    await expect(summary).toHaveText('Will create 1 land parcel, 2 homes and 1 ground lease.', { timeout: 2_000 })
  }).toPass()

  await page.getByRole('button', { name: 'Import', exact: true }).click()
  await expectToast(page, 'Imported 2 homes')
  await expect(summary).toHaveText('Created 1 land parcel, 2 homes and 1 ground lease.')

  await page.locator('.detail-section', { hasText: 'Imported' }).getByRole('link', { name: 'Ground Leases' }).click()
  await expect(async () => {
    await page.goto('/ground-leases')
    await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
    await expect(page.locator('tbody tr')).toHaveCount(1, { timeout: 2_000 })
  }).toPass()

  await page.goto('/portfolio-import')
  await chooseFile(page, portfolio(tag, templateName, homeowner))
  await expect(async () => {
    await page.getByRole('button', { name: 'Check File' }).click()
    await expect(summary).toHaveText(
      'Will create 0 land parcels, 0 homes and 0 ground leases. Already there and skipped: 1 land parcel, 2 homes, 1 ground lease.',
      { timeout: 2_000 })
  }).toPass()
  await expect(page.getByRole('button', { name: 'Import', exact: true })).toBeDisabled()
})
