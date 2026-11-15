import { expect, test, type Page } from '@playwright/test'
import { createGroundLease, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-improvement-request, view-improvement-request,
//   approve-improvement-request, deny-improvement-request; today/work-through-today (improvement items)

const improvementsSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Improvements$/ }) })

test('staff record a roof request with plans, approve it with a credit, and print the letter', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Roof Owner ${uniqueId()}`
  await createGroundLease(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)
  const section = improvementsSection(page)
  await expect(section).toContainText("No improvements cap on this lease, so credits aren't counted")
  await expect(section).toContainText('No improvement requests recorded.')

  await section.getByRole('button', { name: 'Record Request' }).click()
  const record = panel(page, 'Record Improvement Request')
  await field(record, 'Description').fill('Replace the roof')
  await field(record, 'Estimated Cost ($)').fill('18000')
  await field(record, 'Plans or Quotes (optional)').setInputFiles({
    name: 'roof-quote.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 roof quote'),
  })
  await expect(record.getByRole('list', { name: 'Plans to attach' })).toContainText('roof-quote.pdf')
  await record.getByRole('button', { name: 'Record Request', exact: true }).click()
  await expectToast(page, 'Improvement request recorded')

  const request = panel(page, 'Replace the roof')
  await expect(request.locator('.slideover-header')).toContainText(`${homeowner} · Waiting for decision`)
  await expect(detailValue(request, 'Estimated Cost')).toContainText('$18,000.00')
  await expect(detailValue(request, 'Plans')).toContainText('roof-quote.pdf')

  await request.getByRole('button', { name: 'Approve' }).click()
  const approve = panel(page, 'Approve Improvement Request')
  await expect(approve).toContainText('No cap on this lease, so no credit will count.')
  await field(approve, 'Pre-Approved Credit ($)').fill('15000')
  await approve.getByRole('button', { name: 'Approve' }).click()
  await expectToast(page, 'Request approved')
  await expect(request.locator('.slideover-header')).toContainText('Approved')
  await expect(detailValue(request, 'Pre-Approved Credit')).toContainText('$15,000.00')

  const popupPromise = page.waitForEvent('popup')
  await request.getByRole('button', { name: 'Print Letter' }).click()
  const letterPage = await popupPromise
  const letter = letterPage.getByRole('article', { name: 'Approval of improvements to your home' })
  await expect(letter).toContainText(`Dear ${homeowner},`)
  await expect(letter).toContainText('Replace the roof')
  await expect(letter).toContainText('$15,000.00')
  await letterPage.close()

  await request.getByRole('button', { name: 'Close' }).first().click()
  await expect(section.getByRole('table', { name: 'Improvement requests' })).toContainText('Approved')
})

test('a request waiting past two weeks shows on Today as overdue; staff deny it there with a reason', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Addition Owner ${uniqueId()}`
  const leaseId = await createGroundLease(page, homeowner)
  await post(page, `/api/v1/ground-leases/${leaseId}/improvement-requests`, {
    receivedOn: isoDate(-20),
    description: 'Build a garage addition',
    estimatedCost: 40000,
  })

  const todayRow = page.locator('section[aria-labelledby="today-stewardship"] tbody tr', { hasText: homeowner })
    .filter({ hasText: 'Improvement request' })
  await expect(async () => {
    await page.reload()
    await expect(todayRow).toContainText('Improvement request to decide', { timeout: 2000 })
  }).toPass()
  await expect(todayRow).toContainText('Overdue')
  await todayRow.getByRole('button', { name: /^Decide/ }).click()

  const request = panel(page, 'Build a garage addition')
  await expect(request).toContainText('Reply overdue by 6 days')
  await request.getByRole('button', { name: 'Deny' }).click()
  const deny = panel(page, 'Deny Improvement Request')
  await field(deny, 'Reason').fill('The addition would cross the east setback.')
  await deny.getByRole('button', { name: 'Deny' }).click()
  await expectToast(page, 'Request denied')
  await expect(request.locator('.slideover-header')).toContainText('Denied')

  const popupPromise = page.waitForEvent('popup')
  await request.getByRole('button', { name: 'Print Letter' }).click()
  const letterPage = await popupPromise
  const letter = letterPage.getByRole('article', { name: 'Improvements to your home not approved' })
  await expect(letter).toContainText('The addition would cross the east setback.')
  await letterPage.close()

  await request.getByRole('button', { name: 'Close' }).first().click()
  await expect(todayRow).toHaveCount(0)
})

// Journeys: docs/journeys/ground-leases/record-improvement-completed, withdraw-improvement-request

test('staff record finished work with a receipt; the lease counts the credit, then a second request is withdrawn', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Finished Owner ${uniqueId()}`
  const leaseId = await createGroundLease(page, homeowner)
  const { improvementRequestId } = await post<{ improvementRequestId: string }>(
    page, `/api/v1/ground-leases/${leaseId}/improvement-requests`,
    { receivedOn: isoDate(-10), description: 'Replace the furnace', estimatedCost: 9000 },
  )
  await post(page, `/api/v1/improvement-requests/${improvementRequestId}/approve`, {
    approvedOn: isoDate(-5), preApprovedCredit: 6000, notes: null,
  })
  await post(
    page, `/api/v1/ground-leases/${leaseId}/improvement-requests`,
    { receivedOn: isoDate(-2), description: 'Paint the fence', estimatedCost: 800 },
  )

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)
  const section = improvementsSection(page)
  await section.getByRole('table', { name: 'Improvement requests' }).getByText('Replace the furnace').click()

  const request = panel(page, 'Replace the furnace')
  await request.getByRole('button', { name: 'Record Completed' }).click()
  const complete = panel(page, 'Record Improvement Completed')
  await field(complete, 'Actual Cost ($)').fill('8000')
  await expect(field(complete, 'Confirmed Credit ($)')).toHaveValue('6000')
  await field(complete, 'Receipts').setInputFiles({
    name: 'furnace-receipt.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 furnace receipt'),
  })
  await complete.getByRole('button', { name: 'Record Completed' }).click()
  await expectToast(page, 'Improvement recorded')
  await expect(request.locator('.slideover-header')).toContainText('Completed')
  await expect(detailValue(request, 'Confirmed Credit')).toContainText('$6,000.00')
  await expect(detailValue(request, 'Receipts')).toContainText('furnace-receipt.pdf')
  await request.getByRole('button', { name: 'Close' }).first().click()
  await expect(detailValue(section, 'Confirmed Improvements')).toContainText('$6,000.00')

  await section.getByRole('table', { name: 'Improvement requests' }).getByText('Paint the fence').click()
  const fence = panel(page, 'Paint the fence')
  await fence.getByRole('button', { name: 'Withdraw' }).click()
  const withdraw = panel(page, 'Withdraw Improvement Request')
  await field(withdraw, 'Reason').fill('Waiting until spring.')
  await withdraw.getByRole('button', { name: 'Withdraw' }).click()
  await expectToast(page, 'Request withdrawn')
  await expect(fence.locator('.slideover-header')).toContainText('Withdrawn')
})
