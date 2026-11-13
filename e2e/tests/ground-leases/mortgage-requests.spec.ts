import { expect, test, type Page } from '@playwright/test'
import { createGroundLease, createLeaseFromPurchase, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-mortgage-request, view-mortgage-request, approve-mortgage-request,
//   deny-mortgage-request, record-mortgage-loan-closed; today/work-through-today (mortgage items)

const mortgagesSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Mortgages$/ }) })

test('staff record a refinance, approve it within the cap, print the permission, and record it closed', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Refi Owner ${uniqueId()}`
  await createLeaseFromPurchase(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)
  const section = mortgagesSection(page)
  // The purchase loan the lease started with.
  await expect(section.getByRole('table', { name: 'Permitted mortgages' })).toContainText('E2E Lender')

  await section.getByRole('button', { name: 'Record Request' }).click()
  const record = panel(page, 'Record Mortgage Request')
  await field(record, 'Expected Closing').fill(isoDate(7))
  await expect(record).toContainText('The lease asks for at least 15.')
  await field(record, 'Lender').fill('Columbia Bank')
  await field(record, 'Principal ($)').fill('100000')
  await expect(field(record, 'Total Mortgage Debt After Closing ($)')).toHaveValue('100000')
  await field(record, 'Interest Rate (% a year)').fill('6.25')
  await field(record, 'Term (years)').fill('30')
  await field(record, 'Appraised Value ($)').fill('400000')
  await field(record, 'Appraisal Date').fill(isoDate(-3))
  await record.getByRole('button', { name: 'Record Request' }).click()
  await expectToast(page, 'Request recorded')

  const request = panel(page, 'Refinance with Columbia Bank')
  await expect(request.locator('.slideover-header')).toContainText(`${homeowner} · Waiting for decision`)
  await expect(detailValue(request, 'Debt Cap')).toContainText('of the purchase option price')
  await expect(request).toContainText('Within the cap.')

  await request.getByRole('button', { name: 'Approve' }).click()
  const approve = panel(page, 'Approve Mortgage Request')
  await approve.getByRole('button', { name: 'Approve' }).click()
  await expectToast(page, 'Request approved')
  await expect(request.locator('.slideover-header')).toContainText('Approved')

  const popupPromise = page.waitForEvent('popup')
  await request.getByRole('button', { name: 'Print Letter' }).click()
  const letterPage = await popupPromise
  const letter = letterPage.getByRole('article', { name: 'Permission for a mortgage loan' })
  await expect(letter).toContainText(`Dear ${homeowner},`)
  await expect(letter).toContainText('a principal of $100,000.00 at 6.25% fixed, 30 years')
  await expect(letter).toContainText('cc: Columbia Bank')
  await letterPage.close()

  await request.getByRole('button', { name: 'Record Loan Closed' }).click()
  const closed = panel(page, 'Record Loan Closed')
  await expect(field(closed, 'Closed On')).toHaveValue(isoDate())
  // A refinance pays off the current loans by default.
  await expect(closed.getByRole('checkbox', { name: /E2E Lender/ })).toBeChecked()
  await field(closed, 'Notice Address for Columbia Bank').fill('Columbia Bank Servicing\nPO Box 9')
  await closed.getByRole('button', { name: 'Record Loan Closed' }).click()
  await expectToast(page, 'Loan recorded closed')
  await expect(request.locator('.slideover-header')).toContainText('Closed')
  await expect(detailValue(request, 'Paid Off')).toContainText('E2E Lender')
  await request.getByRole('button', { name: 'Close' }).first().click()

  const rows = section.getByRole('table', { name: 'Permitted mortgages' }).locator('tbody tr')
  await expect(rows.filter({ hasText: 'Columbia Bank' })).toContainText('PO Box 9')
  await expect(rows.filter({ hasText: 'Columbia Bank' })).toContainText('Request')
  await expect(rows.filter({ hasText: 'E2E Lender' })).toContainText('Paid off')
  await expect(section.getByRole('table', { name: 'Mortgage requests' })).toContainText('Closed')
})

test('a request without its appraisal waits on Today; staff add it there and deny the loan above the cap', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Over Cap Owner ${uniqueId()}`
  const leaseId = await createGroundLease(page, homeowner)
  await post(page, `/api/v1/ground-leases/${leaseId}/mortgage-requests`, {
    receivedOn: isoDate(),
    lender: 'Quick Loans',
    purpose: 'SecondMortgage',
    principal: 300000,
    totalDebtAfterClosing: 300000,
    interestRatePercent: 9.5,
    rateType: 'Adjustable',
    termYears: 15,
    expectedClosingOn: isoDate(40),
    closingCosts: null,
    appraisedValue: null,
    appraisalDate: null,
    currentIndexValue: null,
    notes: null,
  })

  const todayRow = page.locator('section[aria-labelledby="today-stewardship"] tbody tr', { hasText: homeowner })
  await expect(async () => {
    await page.reload()
    await expect(todayRow).toContainText('Mortgage request to decide', { timeout: 2000 })
  }).toPass()
  await expect(todayRow).toContainText('waiting for the appraisal')
  await todayRow.getByRole('button', { name: /^Add appraisal/ }).click()

  const request = panel(page, 'Second Mortgage with Quick Loans')
  await expect(request).toContainText("The debt check needs the loan's appraisal.")
  await expect(request.getByRole('button', { name: 'Approve' })).toBeDisabled()
  await request.getByRole('button', { name: 'Add Appraisal' }).click()
  const appraisal = panel(page, 'Add Appraisal')
  await field(appraisal, 'Appraised Value ($)').fill('250000')
  await field(appraisal, 'Appraisal Date').fill(isoDate(-1))
  await appraisal.getByRole('button', { name: 'Add Appraisal' }).click()
  await expectToast(page, 'Appraisal added')
  await expect(request).toContainText('Above the cap by')

  await request.getByRole('button', { name: 'Deny' }).click()
  const deny = panel(page, 'Deny Mortgage Request')
  await field(deny, 'Reason').fill('Total debt would be above the purchase option price.')
  await deny.getByRole('button', { name: 'Deny' }).click()
  await expectToast(page, 'Request denied')
  await expect(request.locator('.slideover-header')).toContainText('Denied')

  const popupPromise = page.waitForEvent('popup')
  await request.getByRole('button', { name: 'Print Letter' }).click()
  const letterPage = await popupPromise
  const letter = letterPage.getByRole('article', { name: 'Mortgage loan not permitted' })
  await expect(letter).toContainText('Total debt would be above the purchase option price.')
  await expect(letter).toContainText('cc: Quick Loans')
  await letterPage.close()

  await request.getByRole('button', { name: 'Close' }).first().click()
  await expect(todayRow).toHaveCount(0)
})
