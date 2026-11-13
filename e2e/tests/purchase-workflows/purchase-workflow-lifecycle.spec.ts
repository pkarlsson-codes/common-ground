import { expect, test, type Locator, type Page } from '@playwright/test'
import { createPurchaseWorkflow, get } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/purchase-workflows/
//   view-purchase-workflow-detail, record-eligibility-verified, record-appraisal-completed, record-financing-committed,
//   record-inspection-completed, record-title-cleared, record-lease-executed, schedule-closing,
//   record-purchase-closed, fail-purchase-workflow, withdraw-purchase-workflow

/** Opens a workflow's detail panel via the lease template filter (the template is unique to each test). */
async function openWorkflow(page: Page, templateName: string, buyerName: string) {
  await page.goto('/purchase-workflows')
  await page.getByRole('button', { name: 'List', exact: true }).click()
  await page.getByRole('combobox').filter({ hasText: 'Filter by lease template…' }).click()
  await page.getByPlaceholder('Filter by lease template…').fill(templateName)
  await page.getByRole('listbox').getByRole('option', { name: templateName }).click()
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.getByRole('row', { name: new RegExp(buyerName) }).click()
  return panel(page, buyerName)
}

/** A track's label, prefixed with ✓ once the track is complete. */
const trackLabel = (detail: Locator, label: string) =>
  detail.getByText(new RegExp(`^(✓ )?${label}$`))

/** The header row of a track: its label, summary, and (while pending) Expand/Collapse button. */
const track = (detail: Locator, label: string) => trackLabel(detail, label).locator('xpath=../..')

async function recordTrack(detail: Locator, label: string, fields: Record<string, string>, submit: string, toast: string) {
  await track(detail, label).getByRole('button', { name: 'Expand' }).click()
  for (const [name, value] of Object.entries(fields)) {
    await field(detail, name).fill(value)
  }
  await detail.getByRole('button', { name: submit }).click()
  await expectToast(detail.page(), toast)
}

/** The confirmation dialog opened on top of the detail panel. */
const confirmDialog = (page: Page) =>
  page.getByRole('dialog').filter({ hasText: 'This action cannot be undone.' }).last()

test('eligibility verification judges the verified income by the limit the home selection used', async ({ page }) => {
  const buyer = `Eligibility ${uniqueId()}`
  // The household declares $65,000 for 1 person; the home is at 80% of AMI, and the e2e baseline in effect
  // (effective Jan 1, 2000) allows $70,000 for 1 person and $80,000 for 2.
  const { templateName } = await createPurchaseWorkflow(page, buyer, { fromApplication: true })
  const detail = await openWorkflow(page, templateName, buyer)

  await track(detail, 'Eligibility').getByRole('button', { name: 'Expand' }).click()
  await expect(detail.getByText('Income limit: 80% of AMI, limits effective Jan 1, 2000')).toBeVisible()
  await expect(field(detail, 'Verified Annual Income ($)')).toHaveValue('65000')
  await expect(field(detail, 'Verified Household Size')).toHaveValue('1')
  const limitCheck = detail.getByRole('status').filter({ hasText: /^Limit for/ })
  await expect(limitCheck).toHaveText('Limit for 1 person: $70,000 · Income band: Over 50% to 80% of AMI')

  await field(detail, 'Verified Annual Income ($)').fill('75000')
  await expect(limitCheck).toContainText('Over income limit')
  await expect(limitCheck).toContainText('Income band: Over 80% to 120% of AMI')
  await detail.getByRole('button', { name: 'Record Eligibility' }).click()
  await expectToast(page,
    'The verified income of $75,000 is over the 80% of AMI limit of $70,000 for a household of 1 (limits effective Jan 1, 2000).')
  await expect(track(detail, 'Eligibility')).toContainText('Pending')

  await field(detail, 'Verified Household Size').fill('2')
  await expect(limitCheck).toHaveText('Limit for 2 people: $80,000 · Income band: Over 50% to 80% of AMI')
  await detail.getByRole('button', { name: 'Record Eligibility' }).click()
  await expectToast(page, 'Eligibility recorded')
  await expect(detail.getByText(`Verified ${isoDate()}: $75,000 income, 2 people, within the $80,000 limit · Over 50% to 80% of AMI`)).toBeVisible()
})

test('recording every track, scheduling, and closing a purchase creates the ground lease', async ({ page }) => {
  test.slow()
  await page.goto('/purchase-workflows')
  const buyer = `Buyer ${uniqueId()}`
  const { homeId, templateName } = await createPurchaseWorkflow(page, buyer)

  const detail = await openWorkflow(page, templateName, buyer)
  await expect(detailValue(detail, 'Phase')).toHaveText('Initiated')
  await expect(detailValue(detail, 'Purchase Price')).toHaveText('$200,000')
  await expect(detail.getByRole('button', { name: 'Open Lease Template' })).toBeVisible()
  await expect(detail.getByRole('button', { name: 'Open Land Parcel' })).toBeVisible()
  await expect(detail.getByRole('button', { name: 'Open Home' })).toBeVisible()
  // Initiated directly rather than from an application, so there's nothing to link back to.
  await expect(detail.getByRole('button', { name: 'Open Application' })).toHaveCount(0)
  await expect(detail.getByRole('button', { name: 'Open Household' })).toHaveCount(0)
  for (const label of ['Eligibility', 'Appraisal', 'Financing', 'Inspection', 'Title', 'Lease Execution']) {
    await expect(track(detail, label)).toContainText('Pending')
  }
  // Closing can't be scheduled until the six tracks are complete.
  await expect(trackLabel(detail, 'Closing')).toHaveCount(0)

  const eligibilityDate = isoDate(-10)
  await recordTrack(detail, 'Eligibility', {
    'Verified Date': eligibilityDate,
    'Verified Annual Income ($)': '52000',
    'Verified Household Size': '2',
    'Notes (optional)': 'Income verified',
  }, 'Record Eligibility', 'Eligibility recorded')
  await expect(trackLabel(detail, 'Eligibility')).toHaveText('✓ Eligibility')
  // Initiated directly, so there was no selection and no limit to check against.
  await expect(detail.getByText(`Verified ${eligibilityDate}: $52,000 income, 2 people`, { exact: true })).toBeVisible()
  await expect(detailValue(detail, 'Phase')).toHaveText('In Progress')

  const appraisalDate = isoDate(-9)
  await recordTrack(detail, 'Appraisal',
    { 'Appraised Value ($)': '250000', 'Appraisal Date': appraisalDate, 'Appraiser Name': 'Apple Raiser', 'Notes (optional)': 'Fair condition' },
    'Record Appraisal', 'Appraisal recorded')
  await expect(detail.getByText(`$250,000 · appraised ${appraisalDate} by Apple Raiser`)).toBeVisible()
  await expect(detailValue(detail, 'Appraised Value')).toHaveText('$250,000')

  await recordTrack(detail, 'Financing',
    { 'Lender Name': 'E2E Credit Union', 'Committed Amount ($)': '180000', 'Pre-Approval Letter Date': isoDate(-9) },
    'Record Financing', 'Financing committed')
  await expect(detail.getByText('E2E Credit Union · $180,000 committed')).toBeVisible()

  const inspectionDate = isoDate(-8)
  await recordTrack(detail, 'Inspection', { 'Inspection Date': inspectionDate, 'Inspector Name': 'Ines Pector' },
    'Record Inspection', 'Inspection recorded')
  await expect(detail.getByText(`Inspected ${inspectionDate} by Ines Pector`)).toBeVisible()

  const clearedDate = isoDate(-7)
  await recordTrack(detail, 'Title', { 'Cleared Date': clearedDate, 'Title Company (optional)': 'E2E Title Co' },
    'Record Title Cleared', 'Title cleared')
  await expect(detail.getByText(`Cleared ${clearedDate} · E2E Title Co`)).toBeVisible()

  const signedDate = isoDate(-6)
  await recordTrack(detail, 'Lease Execution', { 'Signed Date': signedDate }, 'Record Lease Executed', 'Lease executed')
  await expect(detail.getByText(`Signed ${signedDate}`)).toBeVisible()

  for (const label of ['Eligibility', 'Appraisal', 'Financing', 'Inspection', 'Title', 'Lease Execution']) {
    await expect(trackLabel(detail, label)).toHaveText(`✓ ${label}`)
    // Completed tracks can't be re-opened.
    await expect(track(detail, label).getByRole('button')).toHaveCount(0)
  }

  await expect(track(detail, 'Closing')).toContainText('Awaiting schedule')
  const scheduledDate = isoDate(7)
  await recordTrack(detail, 'Closing', { 'Scheduled Date': scheduledDate }, 'Schedule Closing', 'Closing scheduled')
  await expect(track(detail, 'Closing')).toContainText(`Scheduled for ${scheduledDate}`)
  await expect(detailValue(detail, 'Phase')).toHaveText('Closing Scheduled')

  const closingDate = isoDate()
  await recordTrack(detail, 'Closing',
    { 'Actual Closing Date': closingDate, 'Closing Notes (optional)': 'Keys handed over' },
    'Record Purchase Closed', 'Purchase closed')
  await expect(detailValue(detail, 'Phase')).toHaveText('Closed')
  await expect(detailValue(detail, 'Closing Notes')).toHaveText('Keys handed over')
  await expect(detail.getByText('Purchase Tracks')).toHaveCount(0)
  await expect(detail.getByText('Exception Actions')).toHaveCount(0)

  // The ground lease is created by an event handler, so poll.
  await expect.poll(async () => {
    const leases = await get<{ items: { homeownerFullName: string; leaseStartDate: string }[] }>(
      page, `/api/v1/ground-leases?homeId=${homeId}`)
    return leases.items.map(l => ({ homeowner: l.homeownerFullName, start: l.leaseStartDate }))
  }).toEqual([{ homeowner: buyer, start: closingDate }])
})

test('failing a purchase asks for confirmation and ends the workflow', async ({ page }) => {
  await page.goto('/purchase-workflows')
  const buyer = `Failing ${uniqueId()}`
  const { templateName } = await createPurchaseWorkflow(page, buyer)

  const detail = await openWorkflow(page, templateName, buyer)
  await field(detail, 'Fail Reason').fill('Financing fell through')
  await detail.getByRole('button', { name: 'Mark as Failed' }).click()

  // Cancelling leaves the workflow untouched.
  await expect(confirmDialog(page)).toContainText('Mark this workflow as failed? This action cannot be undone.')
  await confirmDialog(page).getByRole('button', { name: 'Cancel' }).click()
  await expect(confirmDialog(page)).toBeHidden()
  await expect(detailValue(detail, 'Phase')).toHaveText('Initiated')

  await detail.getByRole('button', { name: 'Mark as Failed' }).click()
  await confirmDialog(page).getByRole('button', { name: 'Mark as Failed' }).click()
  await expectToast(page, 'Workflow marked failed')

  await expect(detailValue(detail, 'Phase')).toHaveText('Failed')
  await expect(detailValue(detail, 'Failure Reason')).toHaveText('Financing fell through')
  await expect(detail.getByText('Purchase Tracks')).toHaveCount(0)
  await expect(detail.getByText('Exception Actions')).toHaveCount(0)

  // The phase filter finds it under Failed, not under Initiated.
  await page.keyboard.press('Escape')
  await expect(detail).toBeHidden()
  const phaseFilter = page.locator('.filter-bar select')
  await phaseFilter.selectOption('Failed')
  await expect(page.getByRole('row', { name: new RegExp(buyer) })).toBeVisible()
  await phaseFilter.selectOption('Initiated')
  await expect(page.locator('.empty-state')).toBeVisible()
})

test('withdrawing a purchase asks for confirmation and ends the workflow', async ({ page }) => {
  await page.goto('/purchase-workflows')
  const buyer = `Withdrawing ${uniqueId()}`
  const { templateName } = await createPurchaseWorkflow(page, buyer)

  const detail = await openWorkflow(page, templateName, buyer)
  await field(detail, 'Withdrawal Reason').fill('Buyer relocated')
  await detail.getByRole('button', { name: 'Withdraw Purchase' }).click()
  await expect(confirmDialog(page)).toContainText('Withdraw this purchase workflow? This action cannot be undone.')
  await confirmDialog(page).getByRole('button', { name: 'Withdraw Purchase' }).click()
  await expectToast(page, 'Workflow withdrawn')

  await expect(detailValue(detail, 'Phase')).toHaveText('Withdrawn')
  await expect(detailValue(detail, 'Withdrawal Reason')).toHaveText('Buyer relocated')
  await expect(detail.getByText('Purchase Tracks')).toHaveCount(0)
  await expect(detail.getByText('Exception Actions')).toHaveCount(0)
})

test('a workflow started by selecting an applicant links back to the application and the household', async ({ page }) => {
  await page.goto('/purchase-workflows')
  const buyer = `Selected ${uniqueId()}`
  const { templateName } = await createPurchaseWorkflow(page, buyer, { fromApplication: true })

  const detail = await openWorkflow(page, templateName, buyer)
  await detail.getByRole('button', { name: 'Open Application' }).click()

  await expect(page).toHaveURL(/\/applications$/)
  await expect(detailValue(panel(page, buyer), 'Phase')).toHaveText('Selected')

  await page.goto('/purchase-workflows')
  const reopened = await openWorkflow(page, templateName, buyer)
  await reopened.getByRole('button', { name: 'Open Household' }).click()

  await expect(page).toHaveURL(/\/households$/)
  await expect(panel(page, buyer)).toContainText('Primary Contact')
})
