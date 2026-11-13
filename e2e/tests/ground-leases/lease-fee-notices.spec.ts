import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, get, post, put } from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/record-lease-fee-notice, docs/journeys/stewardship/send-lease-fee-notices,
//   docs/journeys/trust-policy/set-lease-fee-notice-policy, docs/journeys/today/work-through-today (Stewardship),
//   docs/journeys/ground-leases/open-foreclosure-prevention-case-from-default

/** The 1st of the month `monthsBack` months before the trust's today, as YYYY-MM-DD. */
const firstOfMonth = (monthsBack: number) => {
  const d = new Date(`${isoDate().slice(0, 8)}01T00:00:00Z`)
  d.setUTCMonth(d.getUTCMonth() - monthsBack)
  return d.toISOString().slice(0, 10)
}

const daysSince = (iso: string) => Math.round((Date.parse(`${isoDate()}T00:00:00Z`) - Date.parse(`${iso}T00:00:00Z`)) / 86_400_000)

/**
 * How many months back a lease must start for its oldest charge to be 30 to 59 days overdue, so the second notice is
 * due and the notice of default (60 days) isn't yet. One month back on most days, two early in some months. On the
 * 2nd of March neither fits, and this test needs the policy days changed.
 */
const SECOND_NOTICE_MONTHS = daysSince(firstOfMonth(1)) >= 30 ? 1 : 2

/** What the lease from {@link createLeaseNeedingSecondNotice} owes: $60 for each month since it started. */
const SECOND_NOTICE_BALANCE = `$${(60 * (SECOND_NOTICE_MONTHS + 1)).toFixed(2)}`

/** A $45 + $15 lease whose oldest charge is 30 to 59 days overdue, so a second notice is due. Returns its ID. */
async function createLeaseNeedingSecondNotice(page: Page, homeowner: string) {
  return createLease(page, homeowner, firstOfMonth(SECOND_NOTICE_MONTHS))
}

/** A $45 + $15 lease starting on a given day, with no payments. Returns its ID. */
async function createLease(page: Page, homeowner: string, leaseStartDate: string) {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseId } = await post<{ groundLeaseId: string }>(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    homeownerFullName: homeowner,
    homeownerEmail: `${homeowner.toLowerCase().replace(/\s+/g, '.')}@example.com`,
    homeownerPhone: null,
    leaseStartDate,
    termYears: 99,
    optionWindowDays: 60,
    maxMortgageDebtPercent: 90,
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
  await expect(todayRow).toContainText(`${homeowner}: second notice due, ${SECOND_NOTICE_BALANCE} owed`)
  await todayRow.getByRole('button', { name: /^Record notice/ }).click()

  await expect(page).toHaveURL(/\/ground-leases$/)
  const section = leaseFees(page)
  await expect(section.getByRole('status')).toContainText('Second Notice due')

  const popupPromise = page.waitForEvent('popup')
  await section.getByRole('button', { name: 'Print Letter' }).click()
  const letters = await popupPromise
  const letter = letters.getByRole('article', { name: `Notice to ${homeowner}` })
  await expect(letter).toContainText('Second notice: lease fee past due')
  await expect(letter).toContainText(`Dear ${homeowner},`)
  await expect(letter).toContainText(`you owe ${SECOND_NOTICE_BALANCE}, for the monthly lease fee of $60.00`)
  await expect(letter).toContainText('formal notice of default')
  await letters.close()

  await section.getByRole('button', { name: 'Record Notice' }).click()
  const record = panel(page, 'Record Notice')
  await expect(record.locator('.slideover-header')).toContainText(`${homeowner} · ${SECOND_NOTICE_BALANCE} due`)
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
  await expect(notice).toContainText(SECOND_NOTICE_BALANCE)

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
  const original = await get<{ firstNoticeDays: number; secondNoticeDays: number }>(page, '/api/v1/lease-fee-notice-policy')
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

test('a lease 60 days behind gets a notice of default sent by certified mail, copied to its lenders, whose cure period ends when it is paid', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Default Owner ${uniqueId()}`
  const leaseId = await createLease(page, homeowner, firstOfMonth(3))
  for (const [lender, noticeAddress] of [['Columbia Bank', 'PO Box 12\nSalem OR 97301'], ['Repair Fund', null]])
    await post(page, `/api/v1/ground-leases/${leaseId}/permitted-mortgages`,
      { lender, noticeAddress, originalPrincipal: 150000, permittedOn: firstOfMonth(3) })
  await expect.poll(async () =>
    (await get<{ mortgages?: { permitted?: unknown[] } }>(page, `/api/v1/ground-leases/${leaseId}`)).mortgages?.permitted?.length).toBe(2)
  const cureBy = new Date(`${isoDate(30)}T00:00:00Z`)
    .toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric', timeZone: 'UTC' })

  await openLease(page, homeowner)
  const section = leaseFees(page)
  await expect(section.getByRole('status').first()).toContainText('Notice of Default due')

  const popupPromise = page.waitForEvent('popup')
  await section.getByRole('button', { name: 'Print Letter' }).click()
  const letters = await popupPromise
  const letter = letters.getByRole('article', { name: `Notice to ${homeowner}`, exact: true })
  await expect(letter).toContainText('Notice of default: lease fee unpaid')
  await expect(letter).toContainText(`To cure the default, you must pay the $240.00 owed within 30 days of the date of this notice, by ${cureBy}.`)
  await expect(letter).toContainText('If you pay at least two-thirds of this amount ($160.00)')
  await expect(letters.getByText('1 letter, 2 lender copies')).toBeVisible()
  await expect(letters.getByText('1 lender copy has no notice address on file')).toBeVisible()
  const bankCopy = letters.getByRole('article', { name: `Copy to Columbia Bank of notice to ${homeowner}` })
  await expect(bankCopy).toContainText('Salem OR 97301')
  await expect(bankCopy).toContainText('Notice of default: lease fee unpaid')
  await expect(letters.getByRole('article', { name: `Copy to Repair Fund of notice to ${homeowner}` }))
    .toContainText('Notice address not on file')
  await letters.close()

  await section.getByRole('button', { name: 'Record Notice' }).click()
  const record = panel(page, 'Record Notice')
  await expect(field(record, 'Level')).toHaveValue('Default')
  await expect(field(record, 'Delivery')).toHaveValue('CertifiedMail')
  await expect(field(record, 'Delivery').locator('option')).toHaveText(['Certified Mail', 'Registered Mail', 'Hand Delivered'])
  await record.getByRole('button', { name: 'Record Notice' }).click()
  await expectToast(page, 'Notice recorded')
  await expect(record).toBeHidden()

  await expect(section.getByRole('table', { name: 'Lease fee notices' }).locator('tbody tr').first())
    .toContainText('Columbia Bank, Repair Fund')

  const cure = section.getByRole('status', { name: 'Notice of default' })
  await expect(cure).toContainText(`for $240.00. Cure by ${cureBy}: $240.00 still to pay.`)

  await post(page, `/api/v1/ground-leases/${leaseId}/lease-fee-payments`,
    { receivedDate: isoDate(), amount: 240, method: 'Check', reference: null })
  await page.reload()
  await openLease(page, homeowner)
  await expect(leaseFees(page).getByRole('status', { name: 'Notice of default' })).toContainText('cured: $240.00 paid by')
})

test('a notice of default not cured by its deadline shows on Today and the lease', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Uncured Owner ${uniqueId()}`
  const leaseId = await createLease(page, homeowner, firstOfMonth(3))
  await post(page, `/api/v1/ground-leases/${leaseId}/lease-fee-notices`,
    { level: 'Default', sentOn: isoDate(-35), delivery: 'HandDelivered' })

  await page.reload()
  const todayRow = page.locator('section[aria-labelledby="today-stewardship"] tbody tr', { hasText: 'default not cured' })
    .filter({ hasText: homeowner })
  await expect(todayRow).toContainText('Lease default not cured')
  await expect(todayRow.getByRole('button', { name: /^Review default/ })).toBeVisible()

  await openLease(page, homeowner)
  await expect(leaseFees(page).getByRole('status', { name: 'Notice of default' })).toContainText('not cured by')
})

/** A lease three months behind whose notice of default, sent 35 days ago by hand, wasn't cured by its deadline. */
async function createLeaseWithUncuredDefault(page: Page, homeowner: string) {
  const leaseId = await createLease(page, homeowner, firstOfMonth(3))
  await post(page, `/api/v1/ground-leases/${leaseId}/lease-fee-notices`,
    { level: 'Default', sentOn: isoDate(-35), delivery: 'HandDelivered' })
  return leaseId
}

test('an uncured notice of default opens a foreclosure prevention case in one click, which takes it off Today', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Arrears Owner ${uniqueId()}`
  await createLeaseWithUncuredDefault(page, homeowner)

  await openLease(page, homeowner)
  const box = leaseFees(page).getByRole('status', { name: 'Notice of default' })
  await box.getByRole('button', { name: 'Open Prevention Case' }).click()
  await expectToast(page, 'Foreclosure prevention case opened')
  await expect(box).toContainText('Foreclosure prevention case open since')
  await expect(box.getByRole('button', { name: 'Open Prevention Case' })).toHaveCount(0)

  await box.getByRole('button', { name: 'View Case' }).click()
  await expect(page).toHaveURL(/\/stewardship$/)
  const detail = panel(page, homeowner)
  await expect(detail).toContainText('Lease Fee Arrears · Opened')
  const arrears = detail.getByLabel('Lease fee arrears')
  await expect(arrears).toContainText(`Notice of default sent `)
  await expect(arrears).toContainText(' still to pay when the case opened.')
  await page.keyboard.press('Escape')
  await expect(detail).toBeHidden()

  await page.getByRole('searchbox', { name: 'Search foreclosure prevention cases' }).fill(homeowner)
  const row = page.locator('tbody tr')
  await expect(row).toHaveCount(1)
  await expect(row).toContainText('Lease Fee Arrears')
  await expect(row).toContainText(/Lease fees · \$\d/)

  const todayLoaded = page.waitForResponse(r => r.url().includes('/api/v1/today') && r.ok())
  await page.goto('/')
  await todayLoaded
  await expect(page.locator('tbody tr', { hasText: homeowner })).toHaveCount(0)
})

test('opening a prevention case for a lease that got one meanwhile shows the refusal in the notice of default box', async ({ page }) => {
  await page.goto('/')
  const homeowner = `Raced Owner ${uniqueId()}`
  const leaseId = await createLeaseWithUncuredDefault(page, homeowner)

  await openLease(page, homeowner)
  const box = leaseFees(page).getByRole('status', { name: 'Notice of default' })
  const open = box.getByRole('button', { name: 'Open Prevention Case' })
  await expect(open).toBeVisible()
  await post(page, '/api/v1/stewardship/foreclosure-prevention/lease-default', { groundLeaseId: leaseId })

  await open.click()
  await expect(box.getByRole('alert')).toHaveText('This lease already has an open foreclosure prevention case.')
})
