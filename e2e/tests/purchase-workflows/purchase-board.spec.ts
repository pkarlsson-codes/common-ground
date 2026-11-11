import { expect, test, type Page } from '@playwright/test'
import { createPurchaseWorkflow } from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journey: docs/journeys/purchase-workflows/view-purchase-workflow-detail (the Board)

/** A board column, by its step label. */
const column = (page: Page, step: string) => page.getByRole('region', { name: new RegExp(`^${step} \\d+$`) })

test('the board puts a purchase in the column of the step it waits on, and moves it on once that step is recorded', async ({ page }) => {
  await page.goto('/purchase-workflows')
  const buyer = `Board Buyer ${uniqueId()}`
  const { apn } = await createPurchaseWorkflow(page, buyer)
  const home = `${apn} Elm St`

  await page.goto('/purchase-workflows')
  await expect(page.getByRole('button', { name: 'Board', exact: true })).toHaveAttribute('aria-pressed', 'true')
  for (const step of ['Eligibility', 'Appraisal', 'Financing', 'Inspection', 'Title', 'Lease signing', 'Closing']) {
    await expect(column(page, step)).toBeVisible()
  }
  const card = column(page, 'Eligibility').getByRole('button', { name: new RegExp(buyer) })
  // Homeownership learns the home's label from an integration event, so reload until it has.
  await expect(async () => {
    await page.reload()
    await expect(card).toContainText(home, { timeout: 2_000 })
  }).toPass()
  await expect(card).toContainText('Since today')

  await card.click()
  const detail = panel(page, buyer)
  await detail.getByText(/^Eligibility$/).locator('xpath=../..').getByRole('button', { name: 'Expand' }).click()
  await field(detail, 'Verified Date').fill(isoDate())
  await detail.getByRole('button', { name: 'Record Eligibility' }).click()
  await expectToast(page, 'Eligibility recorded')

  await expect(column(page, 'Appraisal').getByRole('button', { name: new RegExp(buyer) })).toBeVisible()
  await expect(column(page, 'Eligibility').getByRole('button', { name: new RegExp(buyer) })).toHaveCount(0)
})

test('the Board / List choice is remembered', async ({ page }) => {
  await page.goto('/purchase-workflows')
  await page.getByRole('button', { name: 'List', exact: true }).click()
  await expect(page.getByRole('columnheader', { name: 'Waiting On' })).toBeVisible()

  await page.reload()

  await expect(page.getByRole('button', { name: 'List', exact: true })).toHaveAttribute('aria-pressed', 'true')
  await expect(page.getByRole('columnheader', { name: 'Waiting On' })).toBeVisible()
})
