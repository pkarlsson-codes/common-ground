import { expect, test, type Page } from '@playwright/test'
import { createGroundLease } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/open-heir-transfer, view-heir-transfer, record-heir,
//   complete-heir-transfer, close-heir-transfer

const heirSection = (page: Page) =>
  page.locator('.detail-section').filter({ has: page.locator('.detail-section__title', { hasText: /^Heir Transfer$/ }) })

/** The heir transfer view panel; its title is the home's label, so it is found by its Notice section. */
const heirPanel = (page: Page) =>
  page.getByRole('dialog').filter({ has: page.locator('.detail-section__title', { hasText: /^Notice$/ }) }).last()

/** Toasts appear over the panel footer, and a hovering mouse keeps them open, so move away and wait them out. */
async function clearToasts(page: Page) {
  await page.mouse.move(0, 0)
  await expect(page.locator('[data-sonner-toast]')).toHaveCount(0, { timeout: 15000 })
}

async function openLease(page: Page, homeowner: string) {
  await page.goto('/ground-leases')
  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  await panel(page, homeowner)
}

async function openTransfer(page: Page, diedOnOffset: number) {
  const section = heirSection(page)
  await expect(section).toContainText('No heir transfers.')
  await section.getByRole('button', { name: 'Open Heir Transfer' }).click()
  const open = panel(page, 'Open Heir Transfer')
  await field(open, 'Date of Death').fill(isoDate(diedOnOffset))
  await field(open, 'Notified By').fill('Pat Executor')
  return open
}

test('staff open a transfer, record a non-family heir under the income limit, complete it, and print the letter', async ({ page }) => {
  const homeowner = `Late Owner ${uniqueId()}`
  const heir = `Heir ${uniqueId()}`
  await createGroundLease(page, homeowner)
  await openLease(page, homeowner)
  const section = heirSection(page)

  const open = await openTransfer(page, -30)
  await open.getByRole('button', { name: 'Open Transfer' }).click()
  await expectToast(page, 'Heir transfer opened')
  await clearToasts(page)

  const transfer = heirPanel(page)
  await expect(transfer.locator('.slideover-header')).toContainText('Open')
  await expect(detailValue(transfer, 'Notified By')).toContainText('Pat Executor')

  await transfer.getByRole('button', { name: 'Record Heir' }).click()
  const record = panel(page, 'Record Heir')
  await field(record, 'Full Name').fill(heir)
  await field(record, 'Email').fill(`${heir.toLowerCase().replace(/\s+/g, '.')}@example.com`)
  await field(record, 'Relationship').selectOption('Other')
  await field(record, 'Verified Annual Income ($)').fill('50000')
  await field(record, 'Household Size').fill('1')
  await expect(record).toContainText('Limit for 1 person: $70,000.00')
  await expect(record).not.toContainText('Over income limit')
  await field(record, 'Income Documents').setInputFiles({
    name: 'pay-stub.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 pay stub'),
  })
  await record.getByRole('button', { name: 'Record Heir', exact: true }).click()
  await expectToast(page, 'Heir recorded')
  await clearToasts(page)
  await expect(transfer.locator('.slideover-header')).toContainText('Heir recorded')
  await expect(detailValue(transfer, 'Name')).toContainText(heir)
  await expect(detailValue(transfer, 'Verified Income')).toContainText('$50,000.00')
  await expect(detailValue(transfer, 'Income Documents')).toContainText('pay-stub.pdf')

  await transfer.getByRole('button', { name: 'Complete Transfer' }).click()
  const complete = panel(page, 'Complete Heir Transfer')
  await field(complete, 'Signed Letter of Agreement').setInputFiles({
    name: 'agreement.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 agreement'),
  })
  await field(complete, "Signed Attorney's Acknowledgment").setInputFiles({
    name: 'acknowledgement.pdf',
    mimeType: 'application/pdf',
    buffer: Buffer.from('%PDF-1.4 acknowledgement'),
  })
  await complete.getByRole('button', { name: 'Complete Transfer' }).click()
  await expectToast(page, 'Transfer completed')
  await clearToasts(page)
  await expect(transfer.locator('.slideover-header')).toContainText('Completed')
  await expect(detailValue(transfer, 'Signed Letters')).toContainText('Letter of Agreement')
  await expect(detailValue(transfer, 'Signed Letters')).toContainText("Attorney's Acknowledgment")

  const popupPromise = page.waitForEvent('popup')
  await transfer.getByRole('button', { name: 'Print Letter' }).click()
  const letterPage = await popupPromise
  const letter = letterPage.getByRole('article', { name: 'Consent to transfer of your home' })
  await expect(letter).toContainText(`Dear ${heir},`)
  await expect(letter).toContainText(homeowner)
  await letterPage.close()

  await transfer.getByRole('button', { name: 'Close' }).first().click()
  await expect(section.getByRole('table', { name: 'Heir transfers' })).toContainText('Completed')
  await expect(section.getByRole('list', { name: 'Previous homeowners' })).toContainText(homeowner)
})

test('a late notice is flagged, a household member under a year is warned, and staff close the transfer without transfer', async ({ page }) => {
  const homeowner = `Closed Owner ${uniqueId()}`
  await createGroundLease(page, homeowner)
  await openLease(page, homeowner)
  const section = heirSection(page)

  const open = await openTransfer(page, -120)
  await expect(open).toContainText('Late notice')
  await open.getByRole('button', { name: 'Open Transfer' }).click()
  await expectToast(page, 'Heir transfer opened')
  await clearToasts(page)

  const transfer = heirPanel(page)
  await expect(transfer).toContainText('Notice was late')

  await transfer.getByRole('button', { name: 'Record Heir' }).click()
  const record = panel(page, 'Record Heir')
  await field(record, 'Full Name').fill('Sam Roommate')
  await field(record, 'Email').fill(`sam.${uniqueId()}@example.com`)
  await field(record, 'Relationship').selectOption('HouseholdMember')
  await field(record, 'Lived In Home Since').fill(isoDate(-150))
  await expect(record).toContainText('Under one year')
  await record.getByRole('button', { name: 'Cancel' }).click()
  await page.getByRole('dialog').filter({ hasText: /^Discard changes\?/ }).getByRole('button', { name: 'Discard' }).click()
  await expect(record).toBeHidden()

  await transfer.getByRole('button', { name: 'Close Without Transfer' }).click()
  const close = panel(page, 'Close Heir Transfer')
  await field(close, 'Reason').selectOption('HeirNotQualified')
  await close.getByRole('button', { name: 'Close Transfer' }).click()
  await expectToast(page, 'Transfer closed')
  await clearToasts(page)
  await expect(transfer.locator('.slideover-header')).toContainText('Closed')
  await expect(transfer.getByRole('button', { name: 'Complete Transfer' })).toHaveCount(0)

  await transfer.getByRole('button', { name: 'Close' }).first().click()
  await expect(section.getByRole('table', { name: 'Heir transfers' })).toContainText('Closed')
})
