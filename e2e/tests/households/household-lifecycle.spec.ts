import { expect, test, type Page } from '@playwright/test'
import { createApplication, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/households/
//   find-household, view-household-detail, start-application, edit-household,
//   record-housing-counseling, record-clt-orientation, record-mortgage-prequalification

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

test('editing circumstances and adding, editing and removing members', async ({ page }) => {
  const id = uniqueId()
  const primaryName = `Editable ${id}`
  const renamed = `Renamed ${id}`
  const otherName = `Other ${id}`
  const addedName = `Added ${id}`
  await post(page, '/api/v1/households', {
    members: [
      { name: primaryName, email: `editable-${id}@example.com`, phone: '+12025550143', isPrimary: true },
      { name: otherName, email: `other-${id}@example.com`, phone: '+12025550187', isPrimary: false },
    ],
    requiresAccessibility: false,
    annualIncome: 50000,
  })

  await (await findHousehold(page, id, primaryName)).click()
  let detail = panel(page, primaryName)

  // Circumstances
  await detail.getByRole('button', { name: 'Edit Circumstances' }).click()
  const circumstances = detail.getByRole('form', { name: 'Edit Circumstances' })
  await expect(field(circumstances, 'Annual Income ($)')).toHaveValue('50000')
  await field(circumstances, 'Annual Income ($)').fill('81000')
  await circumstances.getByRole('checkbox', { name: 'Accessibility accommodations required' }).check()
  await circumstances.getByRole('button', { name: 'Save Circumstances' }).click()
  await expectToast(page, 'Circumstances updated')
  await expect(circumstances).toBeHidden()
  await expect(detailValue(detail, 'Annual Income')).toHaveText('$81,000')
  await expect(detailValue(detail, 'Accessibility Need')).toHaveText('Yes')

  // The primary contact can be edited but not removed; editing renames the panel and the table row.
  let members = detail.locator('.detail-section', { hasText: 'Household Members' })
  await expect(members.getByRole('button', { name: `Remove ${primaryName}` })).toHaveCount(0)
  await members.getByRole('button', { name: `Edit ${primaryName}` }).click()
  const editPrimary = members.getByRole('form', { name: `Edit ${primaryName}` })
  await expect(field(editPrimary, 'Phone')).toHaveValue('(202) 555-0143')
  await field(editPrimary, 'Full Name').fill(renamed)
  await editPrimary.getByRole('button', { name: 'Save Member' }).click()
  await expectToast(page, 'Member updated')
  detail = panel(page, renamed)
  members = detail.locator('.detail-section', { hasText: 'Household Members' })
  await expect(detailValue(members, 'Primary Contact')).toContainText(renamed)
  await expect(page.getByRole('row', { name: new RegExp(renamed) })).toHaveCount(1)

  // Add a member; an invalid phone is caught inline.
  await members.getByRole('button', { name: 'Add Member' }).click()
  const add = members.getByRole('form', { name: 'New Member' })
  await field(add, 'Full Name').fill(addedName)
  await field(add, 'Email').fill(`added-${id}@example.com`)
  await field(add, 'Phone').fill('123')
  await add.getByRole('button', { name: 'Add Member' }).click()
  await expect(add.getByText('Enter a valid phone number.')).toBeVisible()
  await field(add, 'Phone').fill('2025550199')
  await add.getByRole('button', { name: 'Add Member' }).click()
  await expectToast(page, 'Member added')
  await expect(members).toContainText(addedName)
  await expect(detailValue(detail, 'Household Size')).toHaveText('3')

  // Remove a member, after confirming.
  await members.getByRole('button', { name: `Remove ${otherName}` }).click()
  await expect(members.getByText(`Remove ${otherName} from the household?`)).toBeVisible()
  await members.getByRole('button', { name: 'Confirm Remove' }).click()
  await expectToast(page, 'Member removed')
  await expect(members).not.toContainText(otherName)
  await expect(detailValue(detail, 'Household Size')).toHaveText('2')
})

test('recording readiness on a household without an application, then updating the prequalification', async ({ page }) => {
  const id = uniqueId()
  const primaryName = `Readiness ${id}`
  await post(page, '/api/v1/households', {
    members: [{ name: primaryName, email: `readiness-${id}@example.com`, phone: '+12025550143', isPrimary: true }],
    requiresAccessibility: false,
    annualIncome: 60000,
  })

  const row = await findHousehold(page, id, primaryName)
  await expect(row).toContainText('Not ready')
  await row.click()
  const detail = panel(page, primaryName)
  await expect(detail.getByText('No applications yet.')).toBeVisible()
  const item = (label: string) => detail.locator('.checklist-item', { hasText: label })

  await detail.getByRole('button', { name: 'Record Housing Counseling' }).click()
  const counseling = panel(page, 'Record Housing Counseling')
  await field(counseling, 'Certification Number').fill('CERT-E2E')
  await field(counseling, 'Completed Date').fill(isoDate(-7))
  await field(counseling, 'Counselor Organization').fill('E2E Counseling Org')
  await counseling.getByRole('button', { name: 'Record Counseling' }).click()
  await expectToast(page, 'Housing counseling recorded')
  await expect(item('Housing Counseling')).toHaveClass(/checklist-item--done/)
  await expect(item('Housing Counseling').getByRole('button')).toHaveCount(0)

  await detail.getByRole('button', { name: 'Record CLT Orientation' }).click()
  const orientation = panel(page, 'Record CLT Orientation')
  await field(orientation, 'Completed Date').fill(isoDate(-3))
  await orientation.getByRole('button', { name: 'Record Orientation' }).click()
  await expectToast(page, 'CLT orientation recorded')
  await expect(item('CLT Orientation')).toHaveClass(/checklist-item--done/)
  await expect(item('CLT Orientation').getByRole('button')).toHaveCount(0)

  await detail.getByRole('button', { name: 'Record Mortgage Prequalification' }).click()
  let prequal = panel(page, 'Record Mortgage Prequalification')
  await field(prequal, 'Lender Name').fill('E2E Lender')
  await field(prequal, 'Approved Amount ($)').fill('210000')
  await field(prequal, 'Expiry Date').fill(isoDate(90))
  await prequal.getByRole('button', { name: 'Record Prequalification' }).click()
  await expectToast(page, 'Mortgage prequalification recorded')
  await expect(item('Mortgage Prequalification')).toContainText('E2E Lender · $210,000')
  await expect(row).toContainText('Ready')

  await detail.getByRole('button', { name: 'Update Mortgage Prequalification' }).click()
  prequal = panel(page, 'Record Mortgage Prequalification')
  await field(prequal, 'Lender Name').fill('Second Lender')
  await field(prequal, 'Approved Amount ($)').fill('225000')
  await field(prequal, 'Expiry Date').fill(isoDate(120))
  await prequal.getByRole('button', { name: 'Record Prequalification' }).click()
  await expectToast(page, 'Mortgage prequalification recorded')
  await expect(item('Mortgage Prequalification')).toContainText('Second Lender · $225,000')
})
