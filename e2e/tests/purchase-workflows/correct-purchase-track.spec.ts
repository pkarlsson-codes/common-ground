import { expect, test, type Locator, type Page } from '@playwright/test'
import { createPurchaseWorkflow } from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journey: docs/journeys/purchase-workflows/correct-purchase-track

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

/** The header row of a track: its label, summary and Expand/Correct button. */
const track = (detail: Locator, label: string) =>
  detail.getByText(new RegExp(`^(✓ )?${label}$`)).locator('xpath=../..')

const textFile = (name: string) => ({ name, mimeType: 'text/plain', buffer: Buffer.from(name) })

async function recordAppraisal(detail: Locator, value: string, appraiser: string) {
  await track(detail, 'Appraisal').getByRole('button', { name: 'Expand' }).click()
  await field(detail, 'Appraised Value ($)').fill(value)
  await field(detail, 'Appraisal Date').fill(isoDate(-3))
  await field(detail, 'Appraiser Name').fill(appraiser)
  await detail.getByRole('button', { name: 'Record Appraisal' }).click()
  await expectToast(detail.page(), 'Appraisal recorded')
}

test('a mistyped appraisal is corrected with a reason and the correction is listed', async ({ page }) => {
  const buyer = `Correct Appraisal ${uniqueId()}`
  const { templateName } = await createPurchaseWorkflow(page, buyer)
  const detail = await openWorkflow(page, templateName, buyer)

  await recordAppraisal(detail, '2400000', 'Apple Raiser')
  await expect(detail.getByText(`$2,400,000 · appraised ${isoDate(-3)} by Apple Raiser`)).toBeVisible()
  await expect(detail.getByTestId('purchase-corrections')).toHaveCount(0)

  await track(detail, 'Appraisal').getByRole('button', { name: 'Correct' }).click()
  const form = detail.getByRole('form', { name: 'Correct Appraisal' })
  await expect(field(form, 'Appraised Value ($)')).toHaveValue('2400000')

  // Nothing changed is refused, and the form stays open.
  await field(form, 'Reason').fill('No change')
  await form.getByRole('button', { name: 'Save Correction' }).click()
  await expectToast(page, 'Nothing in the appraisal changed.')

  await field(form, 'Appraised Value ($)').fill('240000')
  await field(form, 'Reason').fill('Typo')
  await form.getByRole('button', { name: 'Save Correction' }).click()
  await expectToast(page, 'Correction saved')

  await expect(detail.getByText(`$240,000 · appraised ${isoDate(-3)} by Apple Raiser`)).toBeVisible()
  const corrections = detail.getByTestId('purchase-corrections')
  await expect(corrections).toContainText('Appraisal')
  await expect(corrections).toContainText('Typo')
  await expect(corrections).toContainText('Appraised value: $2,400,000 → $240,000')
})

test('a corrected eligibility income over the limit is refused and the recorded figures stay', async ({ page }) => {
  const buyer = `Correct Eligibility ${uniqueId()}`
  // The home is at 80% of AMI; the e2e baseline allows $70,000 for 1 person.
  const { templateName } = await createPurchaseWorkflow(page, buyer, { fromApplication: true })
  const detail = await openWorkflow(page, templateName, buyer)

  await track(detail, 'Eligibility').getByRole('button', { name: 'Expand' }).click()
  await field(detail, 'Verified Annual Income ($)').fill('65000')
  await detail.getByLabel('Income Documents').setInputFiles(textFile('pay-stub.txt'))
  await detail.getByRole('button', { name: 'Record Eligibility' }).click()
  await expectToast(page, 'Eligibility recorded')

  await track(detail, 'Eligibility').getByRole('button', { name: 'Correct' }).click()
  const form = detail.getByRole('form', { name: 'Correct Eligibility' })
  await expect(field(form, 'Verified Annual Income ($)')).toHaveValue('65000')
  await field(form, 'Verified Annual Income ($)').fill('75000')
  await expect(form.getByRole('status')).toContainText('Over income limit')
  await field(form, 'Reason').fill('Second job added')
  await form.getByRole('button', { name: 'Save Correction' }).click()
  await expectToast(page, 'The corrected income of $75,000 is over the 80% of AMI limit of $70,000')

  // Within the limit it saves, keeping the recorded documents.
  await field(form, 'Verified Annual Income ($)').fill('68000')
  await form.getByRole('button', { name: 'Save Correction' }).click()
  await expectToast(page, 'Correction saved')
  await expect(detail.getByText(/\$68,000 income/)).toBeVisible()
  await expect(detail.getByTestId('purchase-corrections')).toContainText('Verified annual income: $65,000 → $68,000')
})

test('a financing commitment typed with an extra zero is corrected', async ({ page }) => {
  const buyer = `Correct Financing ${uniqueId()}`
  const { templateName } = await createPurchaseWorkflow(page, buyer)
  const detail = await openWorkflow(page, templateName, buyer)

  await track(detail, 'Financing').getByRole('button', { name: 'Expand' }).click()
  await field(detail, 'Lender Name').fill('E2E Credit Union')
  await field(detail, 'Committed Amount ($)').fill('1800000')
  await field(detail, 'Pre-Approval Letter Date').fill(isoDate(-3))
  await detail.getByRole('button', { name: 'Record Financing' }).click()
  await expectToast(page, 'Financing committed')

  await track(detail, 'Financing').getByRole('button', { name: 'Correct' }).click()
  const form = detail.getByRole('form', { name: 'Correct Financing' })
  await field(form, 'Committed Amount ($)').fill('180000')
  await field(form, 'Reason').fill('Extra zero')
  await form.getByRole('button', { name: 'Save Correction' }).click()
  await expectToast(page, 'Correction saved')
  await expect(detail.getByText('E2E Credit Union · $180,000 committed')).toBeVisible()
  await expect(detail.getByTestId('purchase-corrections')).toContainText('Committed amount: $1,800,000 → $180,000')

})
