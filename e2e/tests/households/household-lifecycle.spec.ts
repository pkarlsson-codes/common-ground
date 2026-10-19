import { expect, test, type Page } from '@playwright/test'
import { createApplication, post } from '../support/api'
import { detailValue, expectToast, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/households/
//   find-household, view-household-detail, start-application

async function findHousehold(page: Page, search: string, primaryName: string) {
  await page.goto('/households')
  await page.getByRole('searchbox', { name: 'Search households' }).fill(search)
  const row = page.getByRole('row', { name: new RegExp(primaryName) })
  await expect(row).toHaveCount(1)
  return row
}

test('search matches any member, and detail shows members, circumstances and readiness', async ({ page }) => {
  const id = uniqueId()
  const primaryName = `Primary ${id}`
  const otherEmail = `member-${id}@example.com`
  await post(page, '/api/v1/households', {
    members: [
      { name: primaryName, email: `primary-${id}@example.com`, phone: '+12025550143', isPrimary: true },
      { name: `Member ${id}`, email: otherEmail, phone: '+12025550187', isPrimary: false },
    ],
    requiresAccessibility: true,
    annualIncome: 72000,
  })

  const row = await findHousehold(page, otherEmail.toUpperCase(), primaryName)
  await expect(row.getByRole('cell').nth(2)).toHaveText('2')
  await expect(row).toContainText('Not ready')

  await row.click()
  const detail = panel(page, primaryName)
  await expect(detailValue(detail, 'Household Size')).toHaveText('2')
  await expect(detailValue(detail, 'Annual Income')).toHaveText('$72,000')
  await expect(detailValue(detail, 'Accessibility Need')).toHaveText('Yes')

  const members = detail.locator('.detail-section', { hasText: 'Household Members' })
  await expect(detailValue(members, 'Primary Contact')).toContainText(primaryName)
  await expect(detailValue(members, 'Member')).toContainText(otherEmail)

  await expect(detail.locator('.checklist .checklist-item--done')).toHaveCount(0)
  await expect(detail.getByText('No applications yet.')).toBeVisible()
  await expect(detail.getByRole('button', { name: 'Start Application' })).toBeVisible()
})

test('household with an open application links to it instead of offering a new one', async ({ page }) => {
  const { name, email } = await createApplication(page, `Open ${uniqueId()}`)

  const row = await findHousehold(page, email, name)
  await expect(row).toContainText('Ready')
  await row.click()
  const detail = panel(page, name)

  await expect(detail.locator('.checklist .checklist-item--done')).toHaveCount(3)
  await expect(detail.getByRole('button', { name: 'Start Application' })).toHaveCount(0)
  await detail.getByRole('button', { name: 'Open Active Application' }).click()

  await expect(page).toHaveURL(/\/applications$/)
  const application = panel(page, name)
  await expect(detailValue(application, 'Phase')).toHaveText('Approved')
})

test('starting an application for a household whose last one was withdrawn', async ({ page }) => {
  const { name, email, applicationId } = await createApplication(page, `Returning ${uniqueId()}`, { approve: false })
  await post(page, `/api/v1/homeowner-applications/${applicationId}/withdraw`, { withdrawnDate: isoDate(), reason: null })

  const row = await findHousehold(page, email, name)
  await row.click()
  const detail = panel(page, name)
  const history = detail.locator('.detail-section', { hasText: 'Applications' }).locator('tbody tr')
  await expect(history).toHaveCount(1)
  await expect(history.first()).toContainText('Withdrawn')

  await detail.getByRole('button', { name: 'Start Application' }).click()
  await expectToast(page, 'Application submitted')

  await expect(history).toHaveCount(2)
  await expect(history.filter({ hasText: 'Submitted' })).toHaveCount(1)
  await expect(detail.getByRole('button', { name: 'Start Application' })).toHaveCount(0)
  await expect(detail.getByRole('button', { name: 'Open Active Application' })).toBeVisible()
})
