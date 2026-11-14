import { expect, test, type Page } from '@playwright/test'
import { createGroundLease, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/set-insurance-requirements, record-insurance-certificate,
//   remove-insurance-certificate, view-ground-lease-detail (Insurance section),
//   docs/journeys/today/work-through-today (Insurance certificate due, Insurance short of the lease)

const todaySection = (page: Page, name: string) => page.getByRole('region', { name: new RegExp(`^${name}`) })

const openLease = async (page: Page, homeowner: string) => {
  await page.goto('/ground-leases')
  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)
}

const insuranceSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Insurance$/ }) })

test('staff set the insurance a lease requires and see the change in its history', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Insured Owner ${uniqueId()}`
  await createGroundLease(page, homeowner)

  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)

  const section = insuranceSection(page)
  await expect(detailValue(section, 'Replacement Value')).toHaveText('Not set')
  await expect(detailValue(section, 'Liability per Occurrence')).toHaveText('Not set')
  await expect(section.getByRole('list', { name: 'Insurance requirements history' })).toHaveCount(0)

  await section.getByRole('button', { name: 'Set Requirements' }).click()
  const set = panel(page, 'Insurance Requirements')
  await expect(field(set, 'Replacement Value ($)')).toHaveValue('')
  await expect(field(set, 'Effective From')).toHaveValue(isoDate())
  await field(set, 'Replacement Value ($)').fill('280000')
  await field(set, 'Liability per Occurrence ($)').fill('300000')
  await set.getByRole('button', { name: 'Save Requirements' }).click()
  await expectToast(page, 'Insurance requirements saved')
  await expect(set).toBeHidden()

  await expect(detailValue(section, 'Replacement Value')).toHaveText('$280,000.00')
  await expect(detailValue(section, 'Liability per Occurrence')).toHaveText('$300,000.00')
  const history = section.getByRole('list', { name: 'Insurance requirements history' }).locator('li')
  await expect(history).toHaveCount(2)
  await expect(history.first()).toContainText('replacement value $280,000.00, liability $300,000.00')
  await expect(history.last()).toContainText('replacement value not set, liability not set')

  // Clearing an amount stops checking it; the form starts from the current amounts.
  await section.getByRole('button', { name: 'Set Requirements' }).click()
  const again = panel(page, 'Insurance Requirements')
  await expect(field(again, 'Liability per Occurrence ($)')).toHaveValue('300000')
  await field(again, 'Liability per Occurrence ($)').fill('')
  await again.getByRole('button', { name: 'Save Requirements' }).click()
  await expectToast(page, 'Insurance requirements saved')
  await expect(detailValue(section, 'Liability per Occurrence')).toHaveText('Not set')
})

test('staff record a certificate with its file, see it checked against the lease, and remove it', async ({ page }) => {
  await page.goto('/ground-leases')
  const homeowner = `Certificate Owner ${uniqueId()}`
  const groundLeaseId = await createGroundLease(page, homeowner)
  await post(page, `/api/v1/ground-leases/${groundLeaseId}/insurance-requirements`, {
    replacementValue: 280000,
    liabilityPerOccurrence: 300000,
    effectiveFrom: isoDate(-365),
  })
  await openLease(page, homeowner)

  const section = insuranceSection(page)
  await expect(detailValue(section, 'Covered Through')).toHaveText('No certificate on file')

  await section.getByRole('button', { name: 'Record Certificate' }).click()
  const record = panel(page, 'Record Insurance Certificate')
  await expect(detailValue(record, 'Replacement Value')).toHaveText('$280,000.00')
  await field(record, 'Insurer').fill('Acme Mutual')
  await field(record, 'Policy Number').fill('HO-12345')
  await field(record, 'Policy Start').fill(isoDate(-10))
  // A one-year policy fills its end.
  await expect(field(record, 'Policy End')).not.toHaveValue('')
  await field(record, 'Policy End').fill(isoDate(355))
  await field(record, 'Dwelling Coverage ($)').fill('250000')
  await field(record, 'Liability per Occurrence ($)').fill('300000')
  const check = record.getByRole('status', { name: 'Check against the lease' })
  await expect(check).toContainText('Dwelling coverage $250,000.00 is below the replacement value $280,000.00')
  await expect(check).toContainText('The trust is not named as additional insured')
  await field(record, 'Trust named as additional insured').check()
  await field(record, 'Dwelling Coverage ($)').fill('280000')
  await expect(check).toHaveText('Meets the lease')
  await field(record, 'Certificate File (optional)').setInputFiles({
    name: 'certificate.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 e2e certificate'),
  })
  await record.getByRole('button', { name: 'Record Certificate' }).click()
  await expectToast(page, 'Insurance certificate recorded')
  await expect(record).toBeHidden()

  await expect(detailValue(section, 'Current Certificate')).toHaveText('Meets the lease')
  await expect(detailValue(section, 'Covered Through')).not.toHaveText('No certificate on file')
  const certificate = section.getByRole('listitem', { name: 'Certificate HO-12345' })
  await expect(certificate).toContainText('Acme Mutual')
  await expect(certificate).toContainText('Dwelling $280,000.00 · liability $300,000.00 · trust named')
  await expect(certificate.getByRole('button', { name: 'Download' })).toBeVisible()

  await certificate.getByRole('button', { name: 'Remove' }).click()
  const remove = panel(page, 'Remove Insurance Certificate')
  await field(remove, 'Reason').fill('Recorded on the wrong lease')
  await remove.getByRole('button', { name: 'Remove Certificate' }).click()
  await expectToast(page, 'Insurance certificate removed')
  await expect(certificate).toContainText('Removed: Recorded on the wrong lease')
  await expect(certificate.getByRole('button', { name: 'Remove' })).toHaveCount(0)
  await expect(detailValue(section, 'Covered Through')).toHaveText('No certificate on file')
})

test('a lease with no certificate waits on Today, and one short of the lease shows there once recorded', async ({ page }) => {
  const homeowner = `Insurance Today ${uniqueId()}`
  await page.goto('/ground-leases')
  await createGroundLease(page, homeowner)

  // Today reads the lease view, which catches up with the new lease after the POST returns.
  const stewardship = todaySection(page, 'Stewardship')
  const due = stewardship.getByRole('row', { name: new RegExp(`Insurance certificate due.*${homeowner}`) })
  await expect(async () => {
    await page.goto('/')
    await expect(due).toContainText(`${homeowner}: no certificate on file`, { timeout: 2_000 })
  }).toPass()

  await due.getByRole('button', { name: /^Record certificate: / }).click()
  const record = panel(page, 'Record Insurance Certificate')
  await field(record, 'Insurer').fill('Acme Mutual')
  await field(record, 'Policy Number').fill('HO-777')
  await field(record, 'Policy Start').fill(isoDate(-5))
  await field(record, 'Dwelling Coverage ($)').fill('250000')
  await field(record, 'Liability per Occurrence ($)').fill('300000')
  await record.getByRole('button', { name: 'Record Certificate' }).click()
  await expectToast(page, 'Insurance certificate recorded')

  await expect(page).toHaveURL(/\/$/)
  await expect(due).toHaveCount(0)
  const short = stewardship.getByRole('row', { name: new RegExp(`Insurance short of the lease.*${homeowner}`) })
  await expect(async () => {
    await page.goto('/')
    await expect(short).toContainText('trust not named as additional insured', { timeout: 2_000 })
  }).toPass()
})
