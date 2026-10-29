import { expect, test, type Page } from '@playwright/test'
import { createApplication, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/applications/
//   submit-application, view-application-detail, approve-application,
//   deny-application, withdraw-application (submit-application covers both New and Existing household)

type Member = { name: string; email: string; phone: string }

const newHousehold = (): { primary: Member; other: Member } => {
  const id = uniqueId()
  return {
    primary: { name: `Primary ${id}`, email: `primary-${id}@example.com`, phone: '(202) 555-0143' },
    other: { name: `Member ${id}`, email: `member-${id}@example.com`, phone: '(202) 555-0187' },
  }
}

async function submitApplication(page: Page, members: Member[], primaryIndex = 0) {
  await page.goto('/applications')
  await page.getByRole('button', { name: 'New Application' }).click()

  const form = panel(page, 'Submit Application')
  for (let i = 0; i < members.length; i++) {
    if (i > 0) await form.getByRole('button', { name: 'Add Member' }).click()
    await field(form, 'Full Name', i).fill(members[i].name)
    await field(form, 'Email', i).fill(members[i].email)
    await field(form, 'Phone', i).fill(members[i].phone)
  }
  await form.getByRole('radio', { name: 'Primary contact' }).nth(primaryIndex).check()
  await field(form, 'Annual Income ($)').fill('65000')

  await expect(form.locator('.form-field', { hasText: 'Household Size' })).toContainText(String(members.length))

  await form.getByRole('button', { name: 'Submit Application' }).click()
  await expectToast(page, 'Application submitted')
  await expect(form).toBeHidden()
}

async function startExistingHousehold(page: Page, search: string, name: string) {
  await page.goto('/applications')
  await page.getByRole('button', { name: 'New Application' }).click()
  const form = panel(page, 'Submit Application')
  await form.getByRole('radio', { name: 'Existing household' }).check()
  await form.getByRole('searchbox', { name: 'Search households' }).fill(search)
  await form.getByRole('button', { name: `Select ${name}` }).click()
  await expect(form.locator('.detail-section', { hasText: 'Selected Household' })).toContainText(name)
  return form
}

async function openApplication(page: Page, primaryName: string) {
  await page.getByRole('row', { name: new RegExp(primaryName) }).click()
  return panel(page, primaryName)
}

test('submitted application shows its declared snapshot, phase-appropriate actions and a link to the household', async ({ page }) => {
  const { primary, other } = newHousehold()
  // Make the second member the primary contact to check the radio is honoured.
  await submitApplication(page, [other, primary], 1)

  const row = page.getByRole('row', { name: new RegExp(primary.name) })
  await expect(row).toContainText(primary.email)
  await expect(row).toContainText('Submitted')
  await expect(row.getByRole('cell').nth(2)).toHaveText('2')

  const detail = await openApplication(page, primary.name)
  await expect(detail.locator('.slideover-header__subtitle')).toHaveText(primary.email)
  await expect(detailValue(detail, 'Phase')).toHaveText('Submitted')
  await expect(detailValue(detail, 'Declared Household Size')).toHaveText('2')
  await expect(detailValue(detail, 'Declared Annual Income')).toHaveText('$65,000')
  await expect(detailValue(detail, 'Declared Accessibility Need')).toHaveText('No')

  // Members and readiness live on the household, not the application.
  await expect(detail.locator('.checklist')).toHaveCount(0)
  await expect(detail.locator('.detail-section', { hasText: 'Household Members' })).toHaveCount(0)

  const actions = detail.locator('.detail-section', { hasText: 'Workflow Actions' })
  await expect(actions.getByRole('button')).toHaveText([
    'Approve Application',
    'Deny Application',
    'Withdraw Application',
  ])

  await detail.getByRole('button', { name: 'Open Household' }).click()
  await expect(page).toHaveURL(/\/households$/)
  const household = panel(page, primary.name)
  const members = household.locator('.detail-section', { hasText: 'Household Members' })
  await expect(detailValue(members, 'Primary Contact')).toContainText(primary.name)
  await expect(detailValue(members, 'Member')).toContainText(other.name)
})

test('approving a submitted application moves it to Approved', async ({ page }) => {
  const { primary } = newHousehold()
  await submitApplication(page, [primary])
  const detail = await openApplication(page, primary.name)

  await detail.getByRole('button', { name: 'Approve Application' }).click()
  const approve = panel(page, 'Approve Application')
  await approve.getByRole('button', { name: 'Approve', exact: true }).click()
  await expectToast(page, 'Application approved')

  await expect(detailValue(detail, 'Phase')).toHaveText('Approved')
  await expect(detailValue(detail, 'Approved Date')).not.toHaveText('—')
  const actions = detail.locator('.detail-section', { hasText: 'Workflow Actions' })
  await expect(actions.getByRole('button')).toHaveText(['Deny Application', 'Withdraw Application'])
})

test('denying a submitted application ends it', async ({ page }) => {
  const { primary } = newHousehold()
  await submitApplication(page, [primary])
  const detail = await openApplication(page, primary.name)

  await detail.getByRole('button', { name: 'Deny Application' }).click()
  const deny = panel(page, 'Deny Application')
  await field(deny, 'Reason').fill('Income above limit')
  await deny.getByRole('button', { name: 'Deny Application' }).click()
  await expectToast(page, 'Application denied')

  await expect(detailValue(detail, 'Phase')).toHaveText('Denied')
  await expect(detailValue(detail, 'Denial Reason')).toHaveText('Income above limit')
  await expect(detail.getByText('No actions available for this phase.')).toBeVisible()
  await expect(detail.locator('.checklist').getByRole('button')).toHaveCount(0)
})

test('withdrawing a submitted application ends it', async ({ page }) => {
  const { primary } = newHousehold()
  await submitApplication(page, [primary])
  const detail = await openApplication(page, primary.name)

  await detail.getByRole('button', { name: 'Withdraw Application' }).click()
  const withdraw = panel(page, 'Withdraw Application')
  await field(withdraw, 'Reason').fill('Moving away')
  await withdraw.getByRole('button', { name: 'Withdraw', exact: true }).click()
  await expectToast(page, 'Application withdrawn')

  await expect(detailValue(detail, 'Phase')).toHaveText('Withdrawn')
  await expect(detailValue(detail, 'Withdrawal Reason')).toHaveText('Moving away')
  await expect(detail.getByText('No actions available for this phase.')).toBeVisible()
})

test('submit is blocked until every member has name, email and phone', async ({ page }) => {
  await page.goto('/applications')
  await page.getByRole('button', { name: 'New Application' }).click()
  const form = panel(page, 'Submit Application')
  const submit = form.getByRole('button', { name: 'Submit Application' })

  await expect(submit).toBeDisabled()
  await field(form, 'Full Name').fill('Only Name')
  await field(form, 'Email').fill('only@example.com')
  await expect(submit).toBeDisabled()
  await field(form, 'Phone').fill('(202) 555-0100')
  await expect(submit).toBeEnabled()

  await form.getByRole('button', { name: 'Add Member' }).click()
  await expect(submit).toBeDisabled()
  await form.getByRole('button', { name: 'Remove' }).nth(1).click()
  await expect(submit).toBeEnabled()
})

test('an existing household applies from intake; one with an open application is blocked', async ({ page }) => {
  const id = uniqueId()

  // A household whose application was withdrawn can apply again.
  const returning = await createApplication(page, `Returning ${id}`, { approve: false })
  await post(page, `/api/v1/homeowner-applications/${returning.applicationId}/withdraw`, { withdrawnDate: isoDate(), reason: null })

  let form = await startExistingHousehold(page, returning.email.toUpperCase(), returning.name)
  await expect(detailValue(form, 'Annual Income')).toHaveText('$65,000')
  await form.getByRole('button', { name: 'Submit Application' }).click()
  await expectToast(page, 'Application submitted')
  await expect(form).toBeHidden()

  const rows = page.getByRole('row', { name: new RegExp(returning.name) })
  await expect(rows).toHaveCount(2)
  await expect(rows.filter({ hasText: 'Submitted' })).toHaveCount(1)

  // Its new application is open, so a second one is blocked and links to it.
  form = await startExistingHousehold(page, returning.email, returning.name)
  await expect(form.getByText('This household already has an open application.')).toBeVisible()
  await expect(form.getByRole('button', { name: 'Submit Application' })).toBeDisabled()
  await form.getByRole('button', { name: 'Open Active Application' }).click()
  await expect(form).toBeHidden()
  await expect(detailValue(panel(page, returning.name), 'Phase')).toHaveText('Submitted')

  // "Change" returns to the search; switching back to New household shows the member form.
  form = await startExistingHousehold(page, returning.email, returning.name)
  await form.getByRole('button', { name: 'Change' }).click()
  await expect(form.getByRole('searchbox', { name: 'Search households' })).toBeVisible()
  await form.getByRole('searchbox', { name: 'Search households' }).fill(`nobody-${id}`)
  await expect(form.getByText(`No households match "nobody-${id}".`)).toBeVisible()
  await form.getByRole('radio', { name: 'New household' }).check()
  await expect(field(form, 'Full Name')).toBeVisible()
  await expect(form.getByRole('button', { name: 'Submit Application' })).toBeDisabled()
})
