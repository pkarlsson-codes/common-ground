import { expect, test, type Page } from '@playwright/test'
import { createGroundLease } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-permitted-mortgage, edit-permitted-mortgage, mark-mortgage-paid-off,
//   view-ground-lease-detail (Mortgages section)

const mortgagesSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Mortgages$/ }) })

test('staff record a permitted mortgage, add its notice address, and mark it paid off', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Mortgage Owner ${uniqueId()}`
  await createGroundLease(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)

  const section = mortgagesSection(page)
  await expect(detailValue(section, 'Debt Cap')).toHaveText('90% of the purchase option price')
  await expect(section).toContainText('No permitted mortgages recorded.')

  await section.getByRole('button', { name: 'Record Mortgage' }).click()
  const record = panel(page, 'Record Permitted Mortgage')
  await field(record, 'Lender or Servicer').fill('First Community Bank')
  await field(record, 'Original Principal ($)').fill('180000')
  await field(record, 'Permitted On').fill(isoDate(-300))
  await record.getByRole('button', { name: 'Record Mortgage' }).click()
  await expectToast(page, 'Mortgage recorded')
  await expect(record).toBeHidden()

  const row = section.getByRole('table', { name: 'Permitted mortgages' }).locator('tbody tr')
  await expect(row).toHaveCount(1)
  await expect(row).toContainText('First Community Bank')
  await expect(row).toContainText('$180,000.00')
  await expect(row).toContainText('Recorded')
  await expect(row).toContainText('Notice address not on file')

  await row.getByRole('button', { name: 'Edit' }).click()
  const edit = panel(page, 'Edit Permitted Mortgage')
  await expect(field(edit, 'Lender or Servicer')).toHaveValue('First Community Bank')
  await field(edit, 'Lender or Servicer').fill('Loan Servicing Co')
  await field(edit, 'Notice Address').fill('Loss Mitigation\n100 Main St')
  await edit.getByRole('button', { name: 'Save' }).click()
  await expectToast(page, 'Mortgage updated')
  await expect(edit).toBeHidden()
  await expect(row).toContainText('Loan Servicing Co')
  await expect(row).toContainText('100 Main St')
  await expect(row).not.toContainText('Notice address not on file')

  await row.getByRole('button', { name: 'Paid Off' }).click()
  const paidOff = panel(page, 'Mark Mortgage Paid Off')
  await field(paidOff, 'Paid Off On').fill(isoDate())
  await paidOff.getByRole('button', { name: 'Mark Paid Off' }).click()
  await expectToast(page, 'Mortgage marked paid off')
  await expect(paidOff).toBeHidden()
  await expect(row).toContainText('Paid off')
  await expect(row.getByRole('button', { name: 'Edit' })).toHaveCount(0)
})
