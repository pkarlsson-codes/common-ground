import { expect, test, type Locator, type Page } from '@playwright/test'
import { createLeaseFromPurchase, createStewardshipCase, get } from '../support/api'
import { expectToast, field, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/stewardship/
//   view-stewardship-cases-queue, view-stewardship-case-detail,
//   initiate-occupancy-verification, submit-occupancy-verification-response, verify-occupancy,
//   mark-occupancy-verification-overdue, reject-occupancy-verification,
//   schedule-property-inspection, complete-property-inspection, cancel-property-inspection,
//   identify-lease-violation, issue-violation-notice, cure-lease-violation, escalate-lease-violation,
//   view-foreclosure-prevention-cases, view-foreclosure-prevention-case-detail, open-foreclosure-prevention-case,
//   record-servicer-delinquency, make-counseling-referral, record-counseling-outcome, request-forbearance,
//   record-forbearance-outcome, activate-cure-period, resolve-foreclosure-prevention-case

async function searchQueue(page: Page, term: string) {
  await page.getByRole('searchbox', { name: 'Search stewardship cases' }).fill(term)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  return page.locator('tbody tr')
}

async function openCase(page: Page, buyer: string) {
  await page.goto('/stewardship')
  const row = await searchQueue(page, buyer)
  await row.click()
  return { row, detail: panel(page, buyer) }
}

const section = (detail: Locator, title: string) =>
  detail.locator('.detail-section').filter({ has: detail.page().locator('.detail-section__title', { hasText: title }) })

test('queue lists the case with homeowner and address, searchable, and links to the lease', async ({ page }) => {
  const buyer = `Steward ${uniqueId()}`
  const { apn } = await createStewardshipCase(page, buyer)

  await page.goto('/stewardship')
  const row = await searchQueue(page, buyer)
  await expect(row).toContainText(`${apn} Elm St`)
  await expect(row).toHaveClass(/queue-row--yellow/) // no verification yet this year

  // Address search finds it too.
  await searchQueue(page, apn)
  await row.click()

  const detail = panel(page, buyer)
  await expect(detail.locator('.slideover-header__subtitle')).toContainText(`${apn} Elm St · Elevated · Opened`)
  await expect(section(detail, 'Occupancy Verification')).toContainText(`No verification started for ${new Date().getFullYear()}.`)
  await expect(section(detail, 'Property Inspection')).toContainText('No inspection scheduled.')
  await expect(section(detail, 'Lease Violations')).toContainText('No active violations.')

  await detail.getByRole('button', { name: 'Open Ground Lease' }).click()
  await expect(page).toHaveURL(/\/ground-leases$/)
  await expect(panel(page, buyer)).toBeVisible()
})

test('occupancy verification is started, answered, and verified', async ({ page }) => {
  const buyer = `Occupant ${uniqueId()}`
  await createStewardshipCase(page, buyer)
  const { row, detail } = await openCase(page, buyer)
  const occupancy = section(detail, 'Occupancy Verification')

  await occupancy.getByRole('button', { name: 'Start verification', exact: true }).click()
  await occupancy.getByRole('button', { name: 'Start Verification', exact: true }).click()
  await expectToast(page, `${new Date().getFullYear()} occupancy verification initiated`)
  await expect(occupancy).toContainText('Awaiting homeowner response.')
  await expect(row).toContainText('Pending')

  await field(occupancy, 'Major Changes (optional)').fill('New deck')
  await occupancy.getByRole('button', { name: 'Submit Response' }).click()
  await expectToast(page, 'Verification response submitted')
  await expect(occupancy).toContainText('Primary resident · Changes: New deck')

  await occupancy.getByRole('button', { name: 'Verify', exact: true }).click()
  await expectToast(page, 'Occupancy verified')
  await expect(occupancy).toContainText('occupancy confirmed')
  await expect(row).toContainText('Verified')
  await expect(row).not.toHaveClass(/queue-row--/)
})

test('an overdue verification turns the case urgent, still takes a late response, and can be rejected', async ({ page }) => {
  const buyer = `Late ${uniqueId()}`
  await createStewardshipCase(page, buyer)
  const { row, detail } = await openCase(page, buyer)
  const occupancy = section(detail, 'Occupancy Verification')

  await occupancy.getByRole('button', { name: 'Start verification', exact: true }).click()
  await occupancy.getByRole('button', { name: 'Start Verification', exact: true }).click()
  await occupancy.getByRole('button', { name: 'Mark Overdue' }).click()
  await expectToast(page, 'Verification marked overdue')
  await expect(occupancy).toContainText('A late response can still be submitted.')
  await expect(row).toHaveClass(/queue-row--red/)
  await expect(detail.locator('.slideover-header__subtitle')).toContainText('Urgent')

  await field(occupancy, 'Sublet Disclosed').check()
  await occupancy.getByRole('button', { name: 'Submit Response' }).click()
  await expectToast(page, 'Verification response submitted')
  await expect(occupancy).toContainText('Sublet disclosed')

  await field(occupancy, 'Reason for Rejection').fill('Unit is sublet full time')
  await occupancy.getByRole('button', { name: 'Reject' }).click()
  await expectToast(page, 'Verification failed')
  await expect(occupancy).toContainText('Unit is sublet full time')
  await expect(row).toContainText('Failed')
})

test('property inspections are scheduled, completed, and cancelled', async ({ page }) => {
  const buyer = `Inspected ${uniqueId()}`
  await createStewardshipCase(page, buyer)
  const { row, detail } = await openCase(page, buyer)
  const inspection = section(detail, 'Property Inspection')

  await inspection.getByRole('button', { name: 'Schedule inspection', exact: true }).click()
  await field(inspection, 'Inspector Name').fill('Pat Inspector')
  await inspection.getByRole('button', { name: 'Schedule Inspection', exact: true }).click()
  await expectToast(page, 'Inspection scheduled')
  await expect(inspection).toContainText('Pat Inspector')
  await expect(row.locator('td').nth(4)).toHaveText(/\d{4}-\d{2}-\d{2}/)

  await field(inspection, 'Condition Notes (optional)').fill('Roof in good shape')
  await inspection.getByRole('button', { name: 'Complete' }).click()
  await expectToast(page, 'Inspection completed')
  await expect(row.locator('td').nth(4)).toHaveText('—')

  await inspection.getByRole('button', { name: 'Schedule inspection', exact: true }).click()
  await field(inspection, 'Inspector Name').fill('Sam Inspector')
  await inspection.getByRole('button', { name: 'Schedule Inspection', exact: true }).click()
  await expectToast(page, 'Inspection scheduled')
  await field(inspection, 'Cancellation Reason').fill('Homeowner travelling')
  await inspection.getByRole('button', { name: 'Cancel', exact: true }).click()
  await expectToast(page, 'Inspection cancelled')

  await inspection.getByRole('button', { name: /Past inspections \(2\)/ }).click()
  await expect(inspection).toContainText('Roof in good shape')
  await expect(inspection).toContainText('Occupancy confirmed')
  await expect(inspection).toContainText('Cancelled: Homeowner travelling')
})

test('lease violations are identified, noticed, then cured or escalated', async ({ page }) => {
  const buyer = `Violator ${uniqueId()}`
  await createStewardshipCase(page, buyer)
  const { row, detail } = await openCase(page, buyer)
  const violations = section(detail, 'Lease Violations')

  const identify = async (type: string, description: string) => {
    await violations.getByRole('button', { name: 'Identify violation', exact: true }).click()
    await field(violations, 'Violation Type').selectOption({ label: type })
    await field(violations, 'Description').fill(description)
    await violations.getByRole('button', { name: 'Identify Violation', exact: true }).click()
    await expectToast(page, 'Violation identified')
  }
  const card = (description: string) => violations.locator('> div').filter({ hasText: description })

  await identify('Unpermitted Alteration', 'Garage converted without permit')
  await expect(row).toHaveClass(/queue-row--orange/)
  await expect(row.locator('td').nth(3)).toHaveText('1')
  const garage = card('Garage converted without permit')
  await expect(garage).toContainText('Unpermitted Alteration')

  await garage.getByRole('button', { name: 'Issue Notice' }).click()
  await expectToast(page, 'Notice issued')
  await expect(garage).toContainText('Cure by')
  await garage.getByRole('button', { name: 'Mark Cured' }).click()
  await expectToast(page, 'Violation cured')
  await expect(row.locator('td').nth(3)).toHaveText('—')

  await identify('Physical Condition', 'Roof collapsing')
  const roof = card('Roof collapsing')
  await roof.getByRole('button', { name: 'Issue Notice' }).click()
  await expectToast(page, 'Notice issued')
  await field(roof, 'Escalation Reason').fill('No repairs after notice')
  await roof.getByRole('button', { name: 'Escalate' }).click()
  await expectToast(page, 'Violation escalated')

  await violations.getByRole('button', { name: /Resolved violations \(2\)/ }).click()
  await expect(violations).toContainText('No repairs after notice')
  await expect(violations).toContainText('No active violations.')
})

test('a foreclosure prevention case is opened, worked, and resolved', async ({ page }) => {
  const buyer = `Delinquent ${uniqueId()}`
  const { apn, leaseId } = await createLeaseFromPurchase(page, buyer)
  const servicer = `Servicer ${uniqueId()}`

  await page.goto('/stewardship')
  await page.getByRole('button', { name: 'Foreclosure Prevention' }).click()
  await page.getByRole('button', { name: 'Open New Case' }).click()
  const form = panel(page, 'Open Foreclosure Prevention Case')
  const open = form.getByRole('button', { name: 'Open Case' })
  await expect(open).toBeDisabled()
  await field(form, 'Ground Lease').click()
  await page.getByPlaceholder('Search by homeowner name or email…').fill(buyer)
  await page.getByRole('listbox').getByRole('option', { name: new RegExp(`^${buyer} — `) }).click()
  await field(form, 'Servicer Name').fill(servicer)
  await field(form, 'Delinquent Months').fill('3')
  await open.click()
  await expectToast(page, 'Foreclosure prevention case opened')
  await expect(form).toBeHidden()

  await page.getByRole('searchbox', { name: 'Search foreclosure prevention cases' }).fill(buyer)
  const row = page.locator('tbody tr')
  await expect(row).toHaveCount(1)
  await expect(row).toContainText(`${apn} Elm St`)
  await expect(row).toContainText('Opened')
  await expect(row).toContainText('Servicer Delinquency')
  await expect(row).toContainText(`${servicer} · 3 mo`)
  await row.click()

  const detail = panel(page, buyer)
  await expect(detail.locator('.slideover-header__subtitle')).toHaveText(`${apn} Elm St · Opened`)
  await expect(detail).toContainText('before resolving the case')
  await expect(detail.getByRole('button', { name: 'Resolve Case' })).toHaveCount(0)

  // Delinquency form defaults to the latest servicer, one month further behind.
  await expect(field(detail, 'Servicer Name')).toHaveValue(servicer)
  await expect(field(detail, 'Delinquent Months')).toHaveValue('4')
  await detail.getByRole('button', { name: 'Record Delinquency' }).click()
  await expectToast(page, 'Delinquency recorded')
  await expect(row).toContainText(`${servicer} · 4 mo`)

  await field(detail, 'Agency Name').fill('Housing Counsel Co')
  await detail.getByRole('button', { name: 'Make Referral' }).click()
  await expectToast(page, 'Referral made')
  await expect(detail.locator('.slideover-header__subtitle')).toHaveText(`${apn} Elm St · Active`)
  await expect(row.locator('td').nth(5)).toHaveText('1')

  const counselingOutcome = detail.getByRole('combobox', { name: 'Counseling outcome' })
  await expect(counselingOutcome.locator('option')).toHaveText(['Counseling Completed', 'Homeowner Unresponsive', 'Not Eligible'])
  await counselingOutcome.selectOption({ label: 'Homeowner Unresponsive' })
  await detail.getByRole('button', { name: 'Record', exact: true }).click()
  await expectToast(page, 'Outcome recorded')
  await expect(detail).toContainText('Outcome: Homeowner Unresponsive')

  await field(detail, 'Lender Name').fill('First Lender')
  await detail.getByRole('button', { name: 'Request Forbearance' }).click()
  await expectToast(page, 'Forbearance requested')
  const forbearanceOutcome = detail.getByRole('combobox', { name: 'Forbearance outcome' })
  await expect(forbearanceOutcome.locator('option')).toHaveText(['Granted', 'Denied'])
  await detail.getByRole('textbox', { name: 'Outcome notes' }).fill('Six months')
  await detail.getByRole('button', { name: 'Record', exact: true }).click()
  await expectToast(page, 'Outcome recorded')
  await expect(detail).toContainText('Outcome: Granted')
  await expect(detail).toContainText('Six months')

  await field(detail, 'LRA ID').fill('LRA-77')
  await detail.getByRole('button', { name: 'Activate Cure Period' }).click()
  await expectToast(page, 'Cure period activated')
  await expect(detail).toContainText('LRA: LRA-77')
  await expect(detail).toContainText('90 days from')

  await field(detail, 'Resolution Outcome').selectOption({ label: 'Cure Period Exercised' })
  await detail.getByRole('button', { name: 'Resolve Case' }).click()
  await expectToast(page, 'Case resolved')
  await expect(detail.locator('.slideover-header__subtitle')).toHaveText(`${apn} Elm St · Resolved`)
  await expect(detail).toContainText('Cure Period Exercised')
  await expect(detail.getByRole('button', { name: 'Make Referral' })).toHaveCount(0)
  await expect(row).toHaveCount(0)

  // The case is still on record for the lease.
  const cases = await get<{ items: { status: string }[] }>(
    page, `/api/v1/stewardship/foreclosure-prevention?groundLeaseId=${leaseId}`)
  expect(cases.items.map(c => c.status)).toEqual(['Resolved'])
})
