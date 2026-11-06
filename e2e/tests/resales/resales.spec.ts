import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-intent-to-sell, docs/journeys/ground-leases/withdraw-intent-to-sell,
//   docs/journeys/resales/view-open-resales

const resaleSection = (page: Page) => page.locator('.detail-section', { hasText: /^Resale/ })

/** A lease created directly, with a 60-day option window, for a homeowner no other test uses. */
async function createLease(page: Page) {
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
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
  })
  return { apn, homeId, homeowner, groundLeaseId }
}

async function openLease(page: Page, homeowner: string) {
  await page.goto('/ground-leases')
  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  return panel(page, homeowner)
}

/** PropertyRegistry hears of resales through integration events, so reload until the home shows the status. */
async function expectHomeStatus(page: Page, apn: string, status: string) {
  await expect(async () => {
    await page.goto('/homes')
    await page.getByRole('combobox').filter({ hasText: 'Filter by parcel…' }).click()
    await page.getByPlaceholder('Filter by parcel…').fill(apn)
    await page.getByRole('listbox').getByRole('option', { name: new RegExp(apn) }).click()
    await expect(page.locator('tbody tr')).toContainText(status, { timeout: 1000 })
  }).toPass()
}

test('recording an intent to sell opens a resale on the lease, lists it on Resales and puts the home in resale until withdrawn', async ({ page }) => {
  const { apn, homeowner } = await createLease(page)

  let detail = await openLease(page, homeowner)
  const section = resaleSection(page)
  await expect(section).toContainText('No resale in progress')
  await section.getByRole('button', { name: 'Record Intent to Sell' }).click()
  await expect(field(section, 'Notice Date')).toHaveValue(isoDate())
  await field(section, 'Notice Date').fill(isoDate(-3))
  await field(section, 'Notice Came By').selectOption('Email')
  await field(section, 'Notes').fill('Relocating for work in the spring')
  await section.getByRole('button', { name: 'Record Intent' }).click()
  await expectToast(page, 'Intent to sell recorded')

  await expect(detailValue(section, 'Notice Date')).toHaveText(isoDate(-3))
  await expect(detailValue(section, 'Notice Came By')).toHaveText('Email')
  await expect(detailValue(section, 'Option Window End')).toHaveText(isoDate(57))
  await expect(detailValue(section, 'Option Window')).toHaveText('57 days left')
  await expect(detailValue(section, 'Notes')).toHaveText('Relocating for work in the spring')
  await expect(section.getByRole('button', { name: 'Record Intent to Sell' })).toHaveCount(0)

  await expectHomeStatus(page, apn, 'Resale')

  await page.goto('/resales')
  const row = page.locator('tbody tr', { hasText: homeowner })
  await expect(row).toContainText(`${apn} Elm St`)
  await expect(row).toContainText(isoDate(-3))
  await expect(row).toContainText('57 days left')
  await row.click()
  await expect(page).toHaveURL(/\/ground-leases$/)
  detail = panel(page, homeowner)
  await expect(detail).toBeVisible()

  await resaleSection(page).getByRole('button', { name: 'Withdraw' }).click()
  await field(resaleSection(page), 'Reason').fill('Decided to stay')
  await resaleSection(page).getByRole('button', { name: 'Withdraw Intent' }).click()
  await expectToast(page, 'Intent to sell withdrawn')
  await expect(resaleSection(page)).toContainText('No resale in progress')
  await expect(resaleSection(page).getByRole('button', { name: 'Record Intent to Sell' })).toBeVisible()

  await expectHomeStatus(page, apn, 'Leased')
  await page.goto('/resales')
  await expect(page.locator('tbody tr', { hasText: homeowner })).toHaveCount(0)
})

test('a resale whose option window has ended says the window passed', async ({ page }) => {
  const { homeowner, groundLeaseId } = await createLease(page)
  await post(page, `/api/v1/ground-leases/${groundLeaseId}/intent-to-sell`, { noticeDate: isoDate(-61), channel: 'Phone', notes: null })

  await openLease(page, homeowner)

  await expect(detailValue(resaleSection(page), 'Option Window End')).toHaveText(isoDate(-1))
  await expect(detailValue(resaleSection(page), 'Option Window')).toHaveText('Window passed')
})

test('a notice dated in the future is refused and the form stays open', async ({ page }) => {
  const { homeowner } = await createLease(page)

  await openLease(page, homeowner)
  const section = resaleSection(page)
  await section.getByRole('button', { name: 'Record Intent to Sell' }).click()
  // The date input caps the date at today; remove the cap to reach the server's rule.
  await field(section, 'Notice Date').evaluate(input => input.removeAttribute('max'))
  await field(section, 'Notice Date').fill(isoDate(1))
  await section.getByRole('button', { name: 'Record Intent' }).click()

  await expect(section.locator('.form-error')).toHaveText("Notice date can't be in the future.")
  await expect(section.getByRole('button', { name: 'Record Intent' })).toBeVisible()
})
