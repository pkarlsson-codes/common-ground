import { readFile } from 'node:fs/promises'
import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, createLeaseFromPurchase, get, post } from '../support/api'
import { field, isoDate, panel, uniqueId } from '../support/ui'

// Journey: docs/journeys/reports/view-funder-report

const section = (page: Page, title: string | RegExp) =>
  page.locator('.report-section', { has: page.getByRole('heading', { name: title, exact: true }) })

async function setPeriod(page: Page, from: string, to: string) {
  await field(page.locator('.filter-bar'), 'From').fill(from)
  await field(page.locator('.filter-bar'), 'To').fill(to)
}

/** A lease from 2020 that the trust buys back at $205,000 after a $270,000 appraisal, closing yesterday. */
async function createBuyback(page: Page) {
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const homeowner = `Seller ${uniqueId()}`
  const { groundLeaseId } = await post<{ groundLeaseId: string }>(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    homeownerFullName: homeowner,
    homeownerEmail: `${homeowner.toLowerCase().replace(/\s+/g, '-')}@example.com`,
    homeownerPhone: null,
    leaseStartDate: '2020-01-01',
    termYears: 99,
    optionWindowDays: 60,
    maxMortgageDebtPercent: 90,
    basePrice: 200000,
    originalAppraisedValue: 250000,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
  })
  const { resaleId } = await post<{ resaleId: string }>(page, `/api/v1/ground-leases/${groundLeaseId}/intent-to-sell`, {
    noticeDate: isoDate(-5), channel: 'Phone', notes: null,
  })
  await post(page, `/api/v1/resales/${resaleId}/appraisal`, {
    appraisedValue: 270000, appraisalDate: isoDate(-4), appraiserName: 'E2E Appraiser', approvedImprovements: null, currentIndexValue: null,
  })
  await post(page, `/api/v1/resales/${resaleId}/exercise-option`, { exercisedDate: isoDate(-3), notes: null })
  await post(page, `/api/v1/resales/${resaleId}/buyback-closing`, {
    closingDate: isoDate(-1), pricePaid: 205000, notes: null, offerForSale: true, holdReason: null,
  })
  return { apn, homeowner, resaleId }
}

test('the period defaults to last calendar year and To before From is refused', async ({ page }) => {
  await page.goto('/')
  await page.getByRole('link', { name: 'Funder report' }).click()
  await expect(page.getByRole('heading', { name: 'Funder Report', level: 1 })).toBeVisible()

  const lastYear = Number(isoDate().slice(0, 4)) - 1
  await expect(field(page.locator('.filter-bar'), 'From')).toHaveValue(`${lastYear}-01-01`)
  await expect(field(page.locator('.filter-bar'), 'To')).toHaveValue(`${lastYear}-12-31`)
  await expect(section(page, `Portfolio on Dec 31, ${lastYear}`)).toBeVisible()

  await field(page.locator('.filter-bar'), 'To').fill(`${lastYear - 1}-12-31`)
  await expect(page.getByRole('alert')).toHaveText('To must be on or after From')
  await expect(page.locator('.report-section')).toHaveCount(0)
})

test('a sale shows its buyer’s income band, a buyback its affordability kept, and both download as CSV', async ({ page }) => {
  const buyer = `Buyer ${uniqueId()}`
  const sale = await createLeaseFromPurchase(page, buyer, { fromApplication: true })
  const buyback = await createBuyback(page)
  const from = isoDate(-30)
  const to = isoDate()

  // Homeownership learns home labels from PropertyRegistry's events, so reload until both rows show them.
  const saleRow = page.locator('.report-section tbody tr', { hasText: `${sale.apn} Elm St` })
  const resaleRow = page.locator('.report-section tbody tr', { hasText: `${buyback.apn} Elm St` })
  await expect(async () => {
    await page.goto('/reports/funder')
    await setPeriod(page, from, to)
    await expect(saleRow).toBeVisible({ timeout: 1000 })
    await expect(resaleRow).toBeVisible({ timeout: 1000 })
  }).toPass()

  await expect(saleRow.locator('td')).toHaveText([
    `${sale.apn} Elm St`, /.*/, '$200,000', '$250,000', '$50,000', '1', '$40,000', 'Up to 50% of AMI',
  ])
  await expect(resaleRow.locator('td')).toHaveText([
    `${buyback.apn} Elm St`, /.*/, '$200,000', '$270,000', '$205,000', '$205,000', '$65,000', '$5,000',
  ])
  await expect(section(page, 'Households by Income Band').locator('tbody tr', { hasText: 'Up to 50% of AMI' })).toBeVisible()

  const report = await get<{ portfolio: { homes: number } }>(page, `/api/v1/reports/funder?from=${from}&to=${to}`)
  await expect(section(page, /^Portfolio on /).locator('tbody td').first()).toHaveText(String(report.portfolio.homes))

  const salesDownload = page.waitForEvent('download')
  await section(page, 'Sales').getByRole('button', { name: 'Download CSV' }).click()
  const salesFile = await salesDownload
  expect(salesFile.suggestedFilename()).toBe(`funder-report-sales-${from}-${to}.csv`)
  const salesCsv = await readFile(await salesFile.path(), 'utf8')
  expect(salesCsv).toContain('Home,Start Date,Price,Appraised Value,Affordability Given,Household Size,Verified Income,Income Band\r\n')
  expect(salesCsv).toContain(`${sale.apn} Elm St,${isoDate(-1)},200000,250000,50000,1,40000,Up to 50% of AMI\r\n`)

  const resalesDownload = page.waitForEvent('download')
  await section(page, 'Resales').getByRole('button', { name: 'Download CSV' }).click()
  const resalesFile = await resalesDownload
  expect(resalesFile.suggestedFilename()).toBe(`funder-report-resales-${from}-${to}.csv`)
  const resalesCsv = await readFile(await resalesFile.path(), 'utf8')
  expect(resalesCsv).toContain(`${buyback.apn} Elm St,${isoDate(-1)},200000,270000,205000,205000,65000,5000\r\n`)

  await saleRow.click()
  await expect(panel(page, buyer)).toBeVisible()
  await page.goBack()
  await setPeriod(page, from, to)
  await resaleRow.click()
  await expect(panel(page, `${buyback.apn} Elm St`)).toBeVisible()
})
