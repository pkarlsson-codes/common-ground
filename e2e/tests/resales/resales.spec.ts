import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, get, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-intent-to-sell, docs/journeys/ground-leases/withdraw-intent-to-sell,
//   docs/journeys/resales/view-open-resales, view-resale-detail, record-resale-appraisal, exercise-purchase-option,
//   record-buyback-closed, record-payoff-statement, docs/journeys/homes/release-home-for-sale

const resaleSection = (page: Page) => page.locator('.detail-section', { hasText: /^Resale(?! Price)/ })

/** A lease created directly, with a 60-day option window, for a homeowner no other test uses. */
async function createLease(page: Page, fees?: { landUseFee: number; repairReserveFee: number; leaseStartDate: string }) {
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const homeowner = `Seller ${uniqueId()}`
  const { groundLeaseId } = await post<{ groundLeaseId: string }>(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    homeownerFullName: homeowner,
    homeownerEmail: `${homeowner.toLowerCase().replace(/\s+/g, '-')}@example.com`,
    homeownerPhone: null,
    leaseStartDate: fees?.leaseStartDate ?? '2020-01-01',
    feesTrackedFrom: fees?.leaseStartDate ?? '2020-01-01',
    openingBalance: 0,
    termYears: 99,
    optionWindowDays: 60,
    maxMortgageDebtPercent: 90,
    ...(fees ? { landUseFee: fees.landUseFee, repairReserveFee: fees.repairReserveFee } : {}),
    basePrice: 200000,
    originalAppraisedValue: 250000,
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
  await expect(detailValue(section, 'Step')).toHaveText('Notice')
  await expect(section.getByRole('button', { name: 'Open Resale' })).toBeVisible()
  await expect(section.getByRole('button', { name: 'Record Intent to Sell' })).toHaveCount(0)

  await expectHomeStatus(page, apn, 'Resale')

  await page.goto('/resales')
  const row = page.locator('tbody tr', { hasText: homeowner })
  await expect(row).toContainText(`${apn} Elm St`)
  await expect(row).toContainText(isoDate(-3))
  await expect(row).toContainText('Notice')
  await expect(row).toContainText('57 days left')
  await row.click()
  await expect(page).toHaveURL(/\/resales$/)
  const resale = panel(page, `${apn} Elm St`)
  await expect(resale).toBeVisible()
  await expect(resale.locator('.next-step-card')).toContainText('Record the appraisal.')

  await resale.getByRole('button', { name: 'More actions' }).click()
  await resale.getByRole('menuitem', { name: 'Withdraw' }).click()
  const withdraw = panel(page, 'Withdraw Intent to Sell')
  await field(withdraw, 'Reason').fill('Decided to stay')
  await withdraw.getByRole('button', { name: 'Withdraw Intent' }).click()
  await expectToast(page, 'Intent to sell withdrawn')

  await expectHomeStatus(page, apn, 'Leased')
  await page.goto('/resales')
  await expect(page.locator('tbody tr', { hasText: homeowner })).toHaveCount(0)

  detail = await openLease(page, homeowner)
  await expect(resaleSection(page)).toContainText('No resale in progress')
  await expect(detail).toBeVisible()
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

test('the trust appraises, buys the home back, holds it and releases it for sale', async ({ page }) => {
  const { apn, homeowner, groundLeaseId } = await createLease(page)
  await post(page, `/api/v1/ground-leases/${groundLeaseId}/intent-to-sell`, { noticeDate: isoDate(-5), channel: 'Phone', notes: null })

  // Homeownership learns the home's label from PropertyRegistry's HomeRegistered event, so reload until it has.
  const resale = panel(page, `${apn} Elm St`)
  await expect(async () => {
    await page.goto('/resales')
    await page.locator('tbody tr', { hasText: homeowner }).click()
    await expect(resale).toBeVisible({ timeout: 1000 })
  }).toPass()
  await resale.getByRole('button', { name: 'Record Appraisal' }).click()

  const appraisal = panel(page, 'Record Appraisal')
  await expect(appraisal.getByRole('button', { name: 'Record Appraisal' })).toBeDisabled()
  await field(appraisal, 'Appraised Value ($)').fill('270000')
  await field(appraisal, 'Appraiser').fill('Hollis Appraisal Group')
  await expect(detailValue(appraisal, 'Formula Price')).toHaveText('$205,000.00')
  await expect(detailValue(appraisal, 'Purchase Option Price')).toHaveText('$205,000.00')
  await appraisal.getByRole('button', { name: 'Record Appraisal' }).click()
  await expectToast(page, 'Appraisal recorded')

  await expect(detailValue(resale, 'Appraiser')).toHaveText('Hollis Appraisal Group')
  await expect(resale.locator('.next-step-card')).toContainText('Decide whether the trust buys this home by')
  await resale.getByRole('button', { name: 'Buy the Home' }).click()

  const exercise = panel(page, 'Buy the Home')
  await expect(detailValue(exercise, 'Purchase Option Price')).toHaveText('$205,000.00')
  await exercise.getByRole('button', { name: 'Exercise Option' }).click()
  await expectToast(page, 'Purchase option exercised')

  await expect(resale.locator('.next-step-card')).toContainText('Record the closing')
  // Withdraw is gone once the option is exercised; only the Payoff Statement is left under More actions.
  await resale.getByRole('button', { name: 'More actions' }).click()
  await expect(page.getByRole('menuitem')).toHaveText(['Payoff Statement'])
  await resale.getByRole('button', { name: 'More actions' }).click()
  await resale.getByRole('button', { name: 'Record Closing' }).click()

  const closing = panel(page, 'Record Closing')
  await expect(field(closing, 'Purchase Price Paid ($)')).toHaveValue('205000')
  await field(closing, 'What Happens Next').selectOption('hold')
  await expect(closing.getByRole('button', { name: 'Record Closing' })).toBeDisabled()
  await field(closing, 'Hold Reason').fill('Roof replacement before it is offered again')
  await closing.getByRole('button', { name: 'Record Closing' }).click()
  await expectToast(page, 'Buyback closed')

  await expect(resale.locator('.next-step-card')).toContainText('The trust bought this home on')
  await expect(detailValue(resale, 'Purchase Price Paid')).toHaveText('$205,000.00')

  await expectHomeStatus(page, apn, 'Held by Trust')
  await page.locator('tbody tr').click()
  const home = page.getByRole('dialog').last()
  await expect(home.locator('.next-step-card')).toContainText('is holding it: Roof replacement before it is offered again.')
  await home.getByRole('button', { name: 'Release for Sale' }).click()
  await expectToast(page, 'Home released for sale')
  await expect(home.locator('.next-step-card')).toContainText('Choose a household from the waiting list')

  const lease = await openLease(page, homeowner)
  await expect(lease.locator('.slideover-header')).toContainText('Inactive')
  await expect(resaleSection(page)).toContainText('The trust bought this home on')
})

test('a payoff statement shows what the seller owes, is recorded when printed, and the closing records it as paid from proceeds', async ({ page }) => {
  // Three monthly charges of $60 are due: this month and the two before it.
  const start = new Date(`${isoDate().slice(0, 8)}01T00:00:00Z`)
  start.setUTCMonth(start.getUTCMonth() - 2)
  const { apn, homeowner, groundLeaseId } = await createLease(page, {
    landUseFee: 45,
    repairReserveFee: 15,
    leaseStartDate: start.toISOString().slice(0, 10),
    feesTrackedFrom: start.toISOString().slice(0, 10),
    openingBalance: 0,
  })
  await post(page, `/api/v1/ground-leases/${groundLeaseId}/intent-to-sell`, { noticeDate: isoDate(-5), channel: 'Phone', notes: null })
  const { items } = await get<{ items: { id: string }[] }>(page, `/api/v1/resales?pageNumber=1&pageSize=1&groundLeaseId=${groundLeaseId}`)
  const resaleId = items[0].id
  await post(page, `/api/v1/resales/${resaleId}/appraisal`, {
    appraisedValue: 270000,
    appraisalDate: isoDate(-2),
    appraiserName: 'Hollis Appraisal Group',
  })
  await post(page, `/api/v1/resales/${resaleId}/exercise-option`, { exercisedDate: isoDate(-1), notes: null })

  const resale = panel(page, `${apn} Elm St`)
  await expect(async () => {
    await page.goto('/resales')
    await page.locator('tbody tr', { hasText: homeowner }).click()
    await expect(resale).toBeVisible({ timeout: 1000 })
  }).toPass()

  await resale.getByRole('button', { name: 'More actions' }).click()
  await page.getByRole('menuitem', { name: 'Payoff Statement' }).click()
  const statement = panel(page, 'Payoff Statement')
  await expect(field(statement, 'Good Through')).toHaveValue(isoDate())
  await expect(statement).toContainText('$180.00')
  await expect(statement).toContainText('Lease fees owed: $180.00')

  const [letter] = await Promise.all([
    page.context().waitForEvent('page'),
    statement.getByRole('button', { name: 'Print Letter' }).click(),
  ])
  await expectToast(page, 'Payoff statement recorded')
  await expect(letter.getByRole('article')).toContainText('the amount owed is $180.00', { timeout: 15000 })
  await letter.close()

  const printed = resale.getByRole('table', { name: 'Payoff statements' }).locator('tbody tr')
  await expect(printed).toHaveCount(1)
  await expect(printed).toContainText(isoDate())
  await expect(printed).toContainText('$180.00')

  await resale.getByRole('button', { name: 'Record Closing' }).click()
  const closing = panel(page, 'Record Closing')
  await expect(field(closing, 'Fees Paid from Proceeds ($)')).toHaveValue('180')
  await closing.getByRole('button', { name: 'Record Closing' }).click()
  await expectToast(page, 'Buyback closed')
  await expect(detailValue(resale, 'Fees Paid from Proceeds')).toHaveText('$180.00')

  const lease = await openLease(page, homeowner)
  const fees = lease.locator('.detail-section', { hasText: 'Lease Fees' })
  const payment = fees.getByRole('table', { name: 'Lease fee payments' }).locator('tbody tr')
  await expect(payment).toHaveCount(1)
  await expect(payment).toContainText('$180.00')
  await expect(payment).toContainText('Sale proceeds')
  await expect(detailValue(fees, 'Balance')).toHaveText('Paid up')
})
