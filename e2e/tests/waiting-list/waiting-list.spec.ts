import { expect, test, type Page } from '@playwright/test'
import { createApplication, post } from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/waiting-list/
//   view-waiting-list

async function searchWaitingList(page: Page, search: string) {
  await page.goto('/waiting-list')
  await page.getByRole('searchbox', { name: 'Search waiting list' }).fill(search)
}

test('approved households are listed with readiness; others are not', async ({ page }) => {
  const id = uniqueId()
  const approved = await createApplication(page, `Waiting ${id}`)
  await createApplication(page, `Submitted ${id}`, { approve: false })

  await searchWaitingList(page, id)
  const rows = page.locator('.registry-table tbody tr')
  await expect(rows).toHaveCount(1)
  const row = rows.first()
  await expect(row.getByRole('cell').nth(0)).toHaveText(approved.name)
  await expect(row.getByRole('cell').nth(3)).toHaveText('0')
  await expect(row).toContainText('Ready')

  await row.click()
  await expect(panel(page, approved.name)).toBeVisible()
})

test('a lapsed prequalification needs re-verification until a new one is recorded', async ({ page }) => {
  const { name, householdId } = await createApplication(page, `Lapsed ${uniqueId()}`)
  await post(page, `/api/v1/households/${householdId}/mortgage-prequalification`, {
    lenderName: 'Old Lender', approvedAmount: 200000, expiryDate: isoDate(-1),
  })

  await searchWaitingList(page, name)
  const row = page.getByRole('row', { name: new RegExp(name) })
  await expect(row).toContainText('Needs re-verification')

  await row.click()
  const detail = panel(page, name)
  // "Record" once the scheduled expiry has cleared the old prequalification, "Update" if it hasn't run yet.
  await detail.getByRole('button', { name: /^(Record|Update) Mortgage Prequalification$/ }).click()
  const prequal = panel(page, 'Record Mortgage Prequalification')
  await field(prequal, 'Lender Name').fill('New Lender')
  await field(prequal, 'Approved Amount ($)').fill('215000')
  await field(prequal, 'Expiry Date').fill(isoDate(90))
  await prequal.getByRole('button', { name: 'Record Prequalification' }).click()
  await expectToast(page, 'Mortgage prequalification recorded')

  await expect(row).toContainText('Ready')
  await expect(row).not.toContainText('Needs re-verification')
})
