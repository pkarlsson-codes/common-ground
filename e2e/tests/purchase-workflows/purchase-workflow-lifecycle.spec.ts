import { expect, test, type Locator, type Page } from '@playwright/test'
import { createPurchaseWorkflow, get } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/purchase-workflows/
//   view-purchase-workflow-detail, record-eligibility-verified, record-financing-committed,
//   record-inspection-completed, record-title-cleared, record-lease-executed, schedule-closing,
//   record-purchase-closed, fail-purchase-workflow, withdraw-purchase-workflow

/** Opens a workflow's detail panel via the lease template filter (the template is unique to each test). */
async function openWorkflow(page: Page, templateName: string, buyerName: string) {
  await page.goto('/purchase-workflows')
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
  for (const label of ['Eligibility', 'Financing', 'Inspection', 'Title', 'Lease Execution']) {
    await expect(track(detail, label)).toContainText('Pending')
  }
  // Closing can't be scheduled until the five tracks are complete.
  await expect(trackLabel(detail, 'Closing')).toHaveCount(0)

  const eligibilityDate = isoDate(-10)
  await recordTrack(detail, 'Eligibility', { 'Verified Date': eligibilityDate, 'Notes (optional)': 'Income verified' },
    'Record Eligibility', 'Eligibility recorded')
  await expect(trackLabel(detail, 'Eligibility')).toHaveText('✓ Eligibility')
  await expect(detail.getByText(`Verified ${eligibilityDate}`)).toBeVisible()
  await expect(detailValue(detail, 'Phase')).toHaveText('In Progress')

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

  for (const label of ['Eligibility', 'Financing', 'Inspection', 'Title', 'Lease Execution']) {
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

test('a workflow started by selecting an applicant links back to the application', async ({ page }) => {
  await page.goto('/purchase-workflows')
  const buyer = `Selected ${uniqueId()}`
  const { templateName } = await createPurchaseWorkflow(page, buyer, { fromApplication: true })

  const detail = await openWorkflow(page, templateName, buyer)
  await detail.getByRole('button', { name: 'Open Application' }).click()

  await expect(page).toHaveURL(/\/applications$/)
  await expect(detailValue(panel(page, buyer), 'Phase')).toHaveText('Selected')
})
