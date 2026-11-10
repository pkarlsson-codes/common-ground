import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, topPanel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-lease-fee-payment, void-lease-fee-payment,
//   view-ground-lease-detail (Lease Fees section); docs/journeys/stewardship/view-lease-fees

/** The 1st of the month `monthsBack` months before the trust's today, as YYYY-MM-DD. */
const firstOfMonth = (monthsBack: number) => {
  const d = new Date(`${isoDate().slice(0, 8)}01T00:00:00Z`)
  d.setUTCMonth(d.getUTCMonth() - monthsBack)
  return d.toISOString().slice(0, 10)
}

/** "February 2026" for a YYYY-MM-DD, as the app shows a due month. */
const monthName = (iso: string) =>
  new Date(`${iso}T00:00:00Z`).toLocaleDateString('en-US', { year: 'numeric', month: 'long', timeZone: 'UTC' })

/** A lease with a $45 + $15 fee whose first charge was two months ago, so three charges are due. */
async function createLeaseOwingThreeMonths(page: Page, homeowner: string) {
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
    basePrice: 200000,
    originalAppraisedValue: 250000,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
  })
}

async function openLease(page: Page, homeowner: string) {
  await page.goto('/ground-leases')
  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  return panel(page, homeowner)
}

const leaseFees = (page: Page) => page.locator('.detail-section', { hasText: 'Lease Fees' })

test('recording a payment lowers the balance, and voiding it restores the balance and keeps it in the history', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Fee Owner ${uniqueId()}`
  await createLeaseOwingThreeMonths(page, homeowner)

  const detail = await openLease(page, homeowner)
  const section = leaseFees(page)
  await expect(detailValue(section, 'Monthly Fee')).toContainText('$60.00')
  await expect(detailValue(section, 'Monthly Fee')).toContainText('$45.00 land use + $15.00 repair reserve')
  await expect(detailValue(section, 'Balance')).toHaveText('$180.00 due')
  await expect(detailValue(section, 'Oldest Unpaid')).toHaveText(monthName(firstOfMonth(2)))
  await expect(section).toContainText('No payments recorded.')

  await section.getByRole('button', { name: 'Record Payment' }).click()
  const record = panel(page, 'Record Payment')
  await expect(record.locator('.slideover-header')).toContainText(`${homeowner} · $180.00 due`)
  await expect(field(record, 'Amount ($)')).toHaveValue('180')
  await expect(field(record, 'Received Date')).toHaveValue(isoDate())
  await field(record, 'Amount ($)').fill('60')
  await field(record, 'Reference').fill('Check #1001')
  await record.getByRole('button', { name: 'Record Payment' }).click()
  await expectToast(page, 'Payment recorded')
  await expect(record).toBeHidden()

  await expect(detailValue(section, 'Balance')).toHaveText('$120.00 due')
  await expect(detailValue(section, 'Oldest Unpaid')).toHaveText(monthName(firstOfMonth(1)))
  const payment = section.getByRole('table', { name: 'Lease fee payments' }).locator('tbody tr')
  await expect(payment).toHaveCount(1)
  await expect(payment).toContainText('$60.00')
  await expect(payment).toContainText('Check #1001')

  await payment.getByRole('button', { name: 'Void' }).click()
  const voidPanel = panel(page, 'Void Payment')
  const voidButton = voidPanel.getByRole('button', { name: 'Void Payment' })
  await expect(voidButton).toBeDisabled()
  await field(voidPanel, 'Reason').fill('Check bounced')
  await voidButton.click()
  await topPanel(page).getByRole('button', { name: 'Void Payment' }).click()
  await expectToast(page, 'Payment voided')
  await expect(voidPanel).toBeHidden()

  await expect(detailValue(section, 'Balance')).toHaveText('$180.00 due')
  await expect(payment).toContainText('Voided: Check bounced')
  await expect(payment.getByRole('button', { name: 'Void' })).toHaveCount(0)
  await expect(detail).toBeVisible()
})

test('the Lease Fees tab lists a lease with a balance due, and its row opens the lease', async ({ page }) => {
  await page.goto('/stewardship')
  const homeowner = `Fee Owner ${uniqueId()}`
  await createLeaseOwingThreeMonths(page, homeowner)

  await page.reload()
  await page.getByRole('button', { name: 'Lease Fees' }).click()
  await page.getByRole('searchbox', { name: 'Search lease fees' }).fill(homeowner)
  const row = page.getByRole('table', { name: 'Lease fee balances' }).locator('tbody tr')
  await expect(row).toHaveCount(1)
  await expect(row).toContainText('$60.00')
  await expect(row).toContainText('$180.00 due')
  await expect(row).toContainText(monthName(firstOfMonth(2)))

  await row.click()
  await expect(panel(page, homeowner)).toBeVisible()
  await expect(detailValue(leaseFees(page), 'Balance')).toHaveText('$180.00 due')
})

test('a paid-up lease is listed on the Lease Fees tab only with All leases', async ({ page }) => {
  await page.goto('/stewardship')
  const homeowner = `Paid Owner ${uniqueId()}`
  await createLeaseOwingThreeMonths(page, homeowner)
  const leases = await (await page.request.get(`/api/v1/lease-fee-balances?includeAll=true&search=${encodeURIComponent(homeowner)}`)).json()
  await post(page, `/api/v1/ground-leases/${leases.items[0].groundLeaseId}/lease-fee-payments`, {
    receivedDate: isoDate(),
    amount: 180,
    method: 'BankTransfer',
  })

  await page.reload()
  await page.getByRole('button', { name: 'Lease Fees' }).click()
  await page.getByRole('searchbox', { name: 'Search lease fees' }).fill(homeowner)
  await expect(page.getByText('No matching leases')).toBeVisible()

  await page.getByRole('combobox', { name: 'Lease fee filter' }).selectOption('all')
  const row = page.getByRole('table', { name: 'Lease fee balances' }).locator('tbody tr')
  await expect(row).toHaveCount(1)
  await expect(row).toContainText('Paid up')
})
