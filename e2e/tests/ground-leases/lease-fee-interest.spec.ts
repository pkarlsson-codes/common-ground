import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, topPanel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/waive-lease-fee-interest, view-ground-lease-detail (Lease Fees section),
//   docs/journeys/stewardship/send-lease-fee-notices (interest in the letter)

/** The 1st of the month `monthsBack` months before the trust's today, as YYYY-MM-DD. */
const firstOfMonth = (monthsBack: number) => {
  const d = new Date(`${isoDate().slice(0, 8)}01T00:00:00Z`)
  d.setUTCMonth(d.getUTCMonth() - monthsBack)
  return d.toISOString().slice(0, 10)
}

const daysSince = (iso: string) => Math.round((Date.parse(`${isoDate()}T00:00:00Z`) - Date.parse(`${iso}T00:00:00Z`)) / 86_400_000)

const dollars = (amount: number) => `$${amount.toFixed(2)}`

/**
 * A $60 lease at 36.5% a year (6 cents a day on an unpaid month) whose oldest charge is two months old. Returns the
 * interest owed today: on each charge more than 30 days late, 6 cents for every day since it fell due.
 */
async function createLeaseOwingInterest(page: Page, homeowner: string) {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  await post(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    homeownerFullName: homeowner,
    homeownerEmail: `${homeowner.toLowerCase().replace(/\s+/g, '.')}@example.com`,
    homeownerPhone: null,
    leaseStartDate: firstOfMonth(2),
    termYears: 99,
    optionWindowDays: 60,
    landUseFee: 45,
    repairReserveFee: 15,
    lateInterestRate: 36.5,
    interestWaiverDays: 30,
    basePrice: 200000,
    originalAppraisedValue: 250000,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
  })
  return [2, 1, 0].map(m => daysSince(firstOfMonth(m))).filter(d => d > 30).reduce((sum, d) => sum + d * 6, 0) / 100
}

test('a late lease shows its interest in the balance and the letter, and waiving it takes it off the balance', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Interest Owner ${uniqueId()}`
  const interest = await createLeaseOwingInterest(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner).waitFor()
  const section = page.locator('.detail-section', { hasText: 'Lease Fees' })
  await expect(detailValue(section, 'Late Interest')).toHaveText('36.5% a year, waived if paid within 30 days')
  await expect(detailValue(section, 'Balance')).toContainText(`${dollars(180 + interest)} due`)
  await expect(detailValue(section, 'Balance')).toContainText(`includes ${dollars(interest)} interest`)

  const popupPromise = page.waitForEvent('popup')
  await section.getByRole('button', { name: 'Print Letter' }).click()
  const letters = await popupPromise
  await expect(letters.getByRole('article', { name: `Notice to ${homeowner}` })).toContainText(
    `This includes ${dollars(interest)} of interest at 36.5% a year on fees not paid within 30 days of their due date.`)
  await letters.close()

  await section.getByRole('button', { name: 'Waive Interest' }).click()
  const waive = panel(page, 'Waive Interest')
  await expect(field(waive, 'Amount ($)')).toHaveValue(String(interest))
  const waiveButton = waive.getByRole('button', { name: 'Waive Interest' })
  await expect(waiveButton).toBeDisabled()
  await field(waive, 'Amount ($)').fill(String(interest + 1))
  await field(waive, 'Reason').fill('Job loss; paying from next month')
  await expect(waive).toContainText(`Can't be more than ${dollars(interest)}.`)
  await expect(waiveButton).toBeDisabled()
  await field(waive, 'Amount ($)').fill(String(interest))
  await waiveButton.click()
  await topPanel(page).getByRole('button', { name: 'Waive Interest' }).click()
  await expectToast(page, 'Interest waived')
  await expect(waive).toBeHidden()

  await expect(detailValue(section, 'Balance')).toHaveText('$180.00 due')
  await expect(section.getByRole('button', { name: 'Waive Interest' })).toHaveCount(0)
  const waiver = section.getByRole('table', { name: 'Interest waivers' }).locator('tbody tr')
  await expect(waiver).toHaveCount(1)
  await expect(waiver).toContainText(dollars(interest))
  await expect(waiver).toContainText('Job loss; paying from next month')
})
