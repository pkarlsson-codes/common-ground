import { expect, test } from '@playwright/test'
import { createLeaseFromPurchase } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/resale-workflows/
//   start-resale-workflow

test('reselling a lease calculates the formula max and starts a purchase workflow for the new buyer', async ({ page }) => {
  test.slow()
  await page.goto('/resale-workflows')
  const owner = `Seller ${uniqueId()}`
  const { leaseId } = await createLeaseFromPurchase(page, owner)

  await page.getByRole('searchbox', { name: 'Search active ground leases' }).fill(owner.toLowerCase())
  await expect(page.locator('tbody tr')).toHaveCount(1)
  const row = page.getByRole('row', { name: new RegExp(owner) })
  await expect(row).toContainText(isoDate(-1))
  await expect(row).toContainText('Appraisal Based')
  await expect(row).toContainText('Active')
  await row.getByRole('button', { name: 'Start Resale' }).click()

  const resale = panel(page, `Resale — ${owner}`)
  const header = resale.locator('.slideover-header')
  await expect(header).toContainText('Step 1 of 3: Intent')
  await expect(detailValue(resale, 'Homeowner')).toHaveText(owner)
  await expect(detailValue(resale, 'Lease Start Date')).toHaveText(isoDate(-1))
  await expect(detailValue(resale, 'Formula')).toHaveText('Appraisal Based')
  await expect(detailValue(resale, 'Ground Lease ID')).toHaveText(leaseId)
  await expect(field(resale, 'Homeowner Notice Date')).toHaveValue(isoDate())
  await resale.getByRole('button', { name: 'Calculate Resale Price →' }).click()

  await expect(header).toContainText('Step 2 of 3: Estimate')
  await expect(resale).toContainText('Original purchase price must be sourced from paper records or closing documents.')
  await field(resale, 'Original Purchase Price ($)').fill('200000')
  await field(resale, 'Current Appraisal Value ($)').fill('300000')
  await field(resale, 'Original Appraisal Value ($)').fill('200000')
  await field(resale, 'Ownership Years').fill('6')
  await resale.getByRole('button', { name: 'Initiate Purchase Workflow →' }).click()

  // 200,000 + 25% of the 100,000 appreciation.
  await expect(header).toContainText('Step 3 of 3: Initiate')
  await expect(field(resale, 'Purchase Price ($)')).toHaveValue('225000')
  await expect(resale).toContainText('Formula max: $225,000')

  // Going back keeps the inputs and shows the calculated max.
  await resale.getByRole('button', { name: 'Back' }).click()
  await expect(header).toContainText('Step 2 of 3: Estimate')
  await expect(resale).toContainText('Max resale: $225,000')
  await expect(field(resale, 'Ownership Years')).toHaveValue('6')
  await resale.getByRole('button', { name: 'Initiate Purchase Workflow →' }).click()

  const buyer = `Buyer ${uniqueId()}`
  await field(resale, 'Purchase Price ($)').fill('220000')
  await field(resale, 'Buyer Full Name').fill(buyer)
  await field(resale, 'Buyer Email').fill('resale-buyer@example.com')
  await field(resale, 'Buyer Phone (optional)').pressSequentially('2025550199')
  await expect(field(resale, 'Buyer Phone (optional)')).toHaveValue('(202) 555-0199')
  await resale.getByRole('button', { name: 'Initiate Workflow' }).click()
  await expectToast(page, 'Purchase workflow initiated')

  await expect(page).toHaveURL(/\/purchase-workflows$/)
  const workflow = panel(page, buyer)
  await expect(detailValue(workflow, 'Phase')).toHaveText('Initiated')
  await expect(detailValue(workflow, 'Purchase Price')).toHaveText('$220,000')
  await expect(workflow.getByRole('button', { name: 'Open Lease Template' })).toBeVisible()
})
