import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, get, post, put } from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-lease-fee-notice, docs/journeys/stewardship/send-lease-fee-notices,
//   docs/journeys/trust-policy/set-lease-fee-notice-policy, docs/journeys/today/work-through-today (Stewardship)

/** The 1st of the month `monthsBack` months before the trust's today, as YYYY-MM-DD. */
const firstOfMonth = (monthsBack: number) => {
  const d = new Date(`${isoDate().slice(0, 8)}01T00:00:00Z`)
  d.setUTCMonth(d.getUTCMonth() - monthsBack)
  return d.toISOString().slice(0, 10)
}

/** A $45 + $15 lease whose oldest charge is two months old, so a second notice is due. Returns its ID. */
async function createLeaseNeedingSecondNotice(page: Page, homeowner: string) {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseId } = await post<{ groundLeaseId: string }>(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    homeownerFullName: homeowner,
    homeownerEmail: `${homeowner.toLowerCase().replace(/\s+/g, '.')}@example.com`,
    homeownerPhone: null,
    leaseStartDate: firstOfMonth(2),
    termYears: 99,
    optionWindowDays: 60,
    landUseFee: 45,
    repairReserveFee: 15,
    basePrice: 200000,
    originalAppraisedValue: 250000,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
  })
  return groundLeaseId
}

async function openLease(page: Page, homeowner: string) {
  await page.goto('/ground-leases')
  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(homeowner)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').click()
  return panel(page, homeowner)
}

const leaseFees = (page: Page) => page.locator('.detail-section', { hasText: 'Lease Fees' })

test('a lease with a notice due shows it on Today and the lease, prints its letter, and records it', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Notice Owner ${uniqueId()}`
  await createLeaseNeedingSecondNotice(page, homeowner)

  await page.reload()
  const todayRow = page.locator('section[aria-labelledby="today-stewardship"] tbody tr', { hasText: homeowner })
  await expect(todayRow).toContainText('Lease fee notice due')
  await expect(todayRow).toContainText(`${homeowner}: second notice due, $180.00 owed`)
  await todayRow.getByRole('button', { name: /^Record notice/ }).click()

  await expect(page).toHaveURL(/\/ground-leases$/)
  const section = leaseFees(page)
  await expect(section.getByRole('status')).toContainText('Second notice due')

  const popupPromise = page.waitForEvent('popup')
  await section.getByRole('button', { name: 'Print Letter' }).click()
  const letters = await popupPromise
  const letter = letters.getByRole('article', { name: `Notice to ${homeowner}` })
  await expect(letter).toContainText('Second notice: lease fee past due')
  await expect(letter).toContainText(`Dear ${homeowner},`)
  await expect(letter).toContainText('you owe $180.00, for the monthly lease fee of $60.00')
  await expect(letter).toContainText('formal notice of default')
  await letters.close()

  await section.getByRole('button', { name: 'Record Notice' }).click()
  const record = panel(page, 'Record Notice')
  await expect(record.locator('.slideover-header')).toContainText(`${homeowner} · $180.00 due`)
  await expect(field(record, 'Level')).toHaveValue('Second')
  await expect(field(record, 'Date Sent')).toHaveValue(isoDate())
  await field(record, 'Delivery').selectOption('CertifiedMail')
  await record.getByRole('button', { name: 'Record Notice' }).click()
  await expectToast(page, 'Notice recorded')
  await expect(record).toBeHidden()

  await expect(section.getByRole('status')).toHaveCount(0)
  const notice = section.getByRole('table', { name: 'Lease fee notices' }).locator('tbody tr')
  await expect(notice).toHaveCount(1)
  await expect(notice).toContainText('Second Notice')
  await expect(notice).toContainText('Certified Mail')
  await expect(notice).toContainText('$180.00')

  await page.goto('/')
  await expect(page.locator('tbody tr', { hasText: homeowner })).toHaveCount(0)
})

test('the Notice due filter prints every letter due and records the ticked ones as sent', async ({ page }) => {
  await page.goto('/stewardship')
  const run = uniqueId()
  const first = `Batch One ${run}`
  const second = `Batch Two ${run}`
  await createLeaseNeedingSecondNotice(page, first)
  await createLeaseNeedingSecondNotice(page, second)

  await page.reload()
  await page.getByRole('button', { name: 'Lease Fees' }).click()
  await page.getByRole('combobox', { name: 'Lease fee filter' }).selectOption('notice')
  await page.getByRole('searchbox', { name: 'Search lease fees' }).fill(run)
  const rows = page.getByRole('table', { name: 'Lease fee balances' }).locator('tbody tr')
  await expect(rows).toHaveCount(2)
  await expect(rows.first()).toContainText('Second due')

  const popupPromise = page.waitForEvent('popup')
  await page.getByRole('button', { name: 'Print Notices' }).click()
  const letters = await popupPromise
  await expect(letters.getByRole('article', { name: `Notice to ${first}` })).toBeVisible()
  await expect(letters.getByRole('article', { name: `Notice to ${second}` })).toBeVisible()
  await letters.close()

  await page.getByRole('button', { name: 'Record Notices Sent' }).click()
  const record = panel(page, 'Record Notices Sent')
  // Other tests share the trust; record only this test's two leases.
  const boxes = record.getByRole('checkbox')
  await expect(record.getByRole('checkbox', { name: `Sent to ${first}` })).toBeVisible()
  for (const box of await boxes.all()) {
    const name = await box.getAttribute('aria-label')
    if (name !== `Sent to ${first}` && name !== `Sent to ${second}`) await box.uncheck()
  }
  await field(record, 'Delivery').selectOption('HandDelivered')
  await record.getByRole('button', { name: 'Record 2 Notices' }).click()
  await expectToast(page, '2 notices recorded')
  await expect(record).toBeHidden()

  await expect(page.getByText('No matching leases')).toBeVisible()
  await page.getByRole('combobox', { name: 'Lease fee filter' }).selectOption('due')
  await expect(rows).toHaveCount(2)
  await expect(rows.first()).toContainText('Second sent')
})

test('the trust policy page saves the letter sign-off and refuses a second notice not after the first', async ({ page }) => {
  await page.goto('/')
  const original = await get<{ firstNoticeDays: number; secondNoticeDays: number; letterSignOff: string }>(page, '/api/v1/lease-fee-notice-policy')
  const homeowner = `Policy Owner ${uniqueId()}`
  const leaseId = await createLeaseNeedingSecondNotice(page, homeowner)
  const signOff = `Riverbend CLT ${uniqueId()}`

  try {
    await page.getByRole('link', { name: 'Trust policy' }).click()
    await expect(page.getByRole('heading', { name: 'Trust Policy' })).toBeVisible()
    const save = page.getByRole('button', { name: 'Save Policy' })
    const form = page.locator('form')
    await expect(field(form, 'First Notice After (days overdue)')).toHaveValue(String(original.firstNoticeDays))

    await field(form, 'Second Notice After (days overdue)').fill(String(original.firstNoticeDays))
    await expect(form).toContainText('Must be more than First Notice After.')
    await expect(save).toBeDisabled()
    await field(form, 'Second Notice After (days overdue)').fill(String(original.secondNoticeDays))

    await field(form, 'Letter Sign-Off').fill(`${signOff}\n(503) 555-0199`)
    await save.click()
    await expectToast(page, 'Policy saved')

    await page.goto(`/lease-fee-notices/print?lease=${leaseId}`)
    const letter = page.getByRole('article', { name: `Notice to ${homeowner}` })
    await expect(letter).toContainText(signOff)
    await expect(letter).toContainText('(503) 555-0199')
  } finally {
    await put(page, '/api/v1/lease-fee-notice-policy', original)
  }
})
