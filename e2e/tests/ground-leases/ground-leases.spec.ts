import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, createLeaseFromPurchase, post } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-leases/
//   create-ground-lease, view-ground-lease-detail, record-stewardship-check-in, start-resale-workflow-from-lease

/**
 * A lease created directly has no homeowner to search by, so it's found by its home: the table lists newest leases
 * first, and the Home cell carries the full home ID as its title.
 */
const leaseRowByHome = (page: Page, homeId: string) =>
  page.locator('tbody tr', { has: page.locator(`[title="${homeId}"]`) })

async function searchLeases(page: Page, term: string) {
  await page.getByRole('searchbox', { name: 'Search ground leases' }).fill(term)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  return page.locator('tbody tr')
}

const checkIns = (page: Page) => page.locator('.detail-section', { hasText: 'Stewardship Check-Ins' })

test('creating a lease on a parcel\'s home lists it first and shows its terms in the detail panel', async ({ page }) => {
  await page.goto('/ground-leases')
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)

  await page.getByRole('button', { name: 'Create Lease' }).click()
  const form = panel(page, 'Create Ground Lease')
  const create = form.getByRole('button', { name: 'Create Lease' })
  await expect(create).toBeDisabled()
  await expect(field(form, 'Home')).toBeDisabled()
  await expect(field(form, 'Lease Start Date')).toHaveValue(isoDate())
  await expect(field(form, 'Term (years)')).toHaveValue('99')

  await field(form, 'Land Parcel').click()
  await page.getByPlaceholder('Search parcels…').fill(apn)
  await page.getByRole('listbox').getByRole('option', { name: `${apn} — ${apn} Elm St` }).click()
  await expect(create).toBeDisabled()
  await field(form, 'Home').click()
  await page.getByRole('listbox').getByRole('option', { name: 'SingleFamily, 3bd/2ba, 1200 sqft' }).click()
  await expect(create).toBeEnabled()

  await field(form, 'Lease Start Date').fill('2023-06-01')
  await field(form, 'Term (years)').fill('50')
  await field(form, 'Covenant Terms').fill('Owner-occupied, permanently affordable')
  await field(form, 'Resale Formula').selectOption('FixedPercentage')
  await expect(field(form, 'Annual Percentage')).toHaveValue('0.01')
  await field(form, 'Annual Percentage').fill('0.02')
  await form.getByRole('checkbox', { name: 'Set improvements cap' }).check()
  await field(form, 'Improvements Cap ($)').fill('10000')
  await create.click()
  await expectToast(page, 'Ground lease created')
  await expect(form).toBeHidden()

  // Created directly, so there's no homeowner yet.
  const row = leaseRowByHome(page, homeId)
  await expect(row).toContainText('—')
  await expect(row).toContainText('2023-06-01')
  await expect(row).toContainText('50 yr')
  await expect(row).toContainText('Active')

  await row.click()
  const detail = panel(page, 'Ground Lease')
  await expect(detail.locator('.slideover-header')).toContainText('2023-06-01 · 50 yr · Active')
  await expect(detailValue(detail, 'Formula')).toHaveText('Fixed Percentage')
  await expect(detailValue(detail, 'Annual %')).toHaveText('2.0%')
  await expect(detailValue(detail, 'Improvements Cap')).toHaveText('$10,000')
  await expect(detailValue(detail, 'Term')).toHaveText('50 years')
  await expect(detailValue(detail, 'Covenant Terms')).toHaveText('Owner-occupied, permanently affordable')
  await expect(detail.locator('.detail-field__label').filter({ hasText: /^(Email|Phone|Appreciation Share)$/ })).toHaveCount(0)
  await expect(checkIns(page)).not.toContainText('by ')

  await detail.getByRole('button', { name: 'Open Land Parcel' }).click()
  await expect(page).toHaveURL(/\/land-parcels$/)
  await expect(panel(page, apn)).toBeVisible()
})

test('a lease created from a closed purchase shows its homeowner and records check-ins newest first', async ({ page }) => {
  await page.goto('/ground-leases')
  const buyer = `Owner ${uniqueId()}`
  const { buyerEmail } = await createLeaseFromPurchase(page, buyer)

  // Search matches the homeowner's email, ignoring case.
  const row = await searchLeases(page, buyerEmail.toUpperCase())
  await expect(row).toContainText(buyer)
  await row.click()

  const detail = panel(page, buyer)
  await expect(detail.locator('.slideover-header')).toContainText(`${isoDate(-1)} · 99 yr · Active`)
  await expect(detailValue(detail, 'Formula')).toHaveText('Appraisal Based')
  await expect(detailValue(detail, 'Appreciation Share')).toHaveText('25.0%')
  await expect(detailValue(detail, 'Email')).toHaveText(buyerEmail)
  await expect(detailValue(detail, 'Phone')).toHaveText('(202) 555-0143')
  await expect(detailValue(detail, 'Covenant Terms')).toHaveText('Standard covenant terms')

  const notes = field(detail, 'Notes')
  await expect(field(detail, 'Check-In Date')).toHaveValue(isoDate())
  await field(detail, 'Check-In Date').fill(isoDate(-5))
  await notes.fill('Roof looks good')
  await detail.getByRole('button', { name: 'Record Check-In' }).click()
  await expectToast(page, 'Check-in recorded')
  await expect(notes).toHaveValue('')
  await expect(field(detail, 'Check-In Date')).toHaveValue(isoDate())

  // An earlier visit recorded afterwards still lists below the more recent one.
  await field(detail, 'Check-In Date').fill(isoDate(-60))
  await notes.fill('Annual walkthrough')
  await detail.getByRole('button', { name: 'Record Check-In' }).click()
  await expectToast(page, 'Check-in recorded')

  const entries = checkIns(page).locator('div', { hasText: /^by / }).locator('..')
  await expect(entries).toHaveCount(2)
  await expect(entries.nth(0)).toContainText(isoDate(-5))
  await expect(entries.nth(0)).toContainText('Roof looks good')
  await expect(entries.nth(1)).toContainText(isoDate(-60))
  await expect(entries.nth(1)).toContainText('Annual walkthrough')

  await detail.getByRole('button', { name: 'Start Resale Workflow' }).click()
  await expect(page).toHaveURL(/\/resale-workflows$/)
  const resale = panel(page, `Resale — ${buyer}`)
  await expect(resale.locator('.slideover-header')).toContainText('Step 1 of 3: Intent')
  await expect(detailValue(resale, 'Homeowner')).toHaveText(buyer)
  await expect(detailValue(resale, 'Formula')).toHaveText('Appraisal Based')
})

test('a lease created directly can be estimated but not resold through a purchase workflow', async ({ page }) => {
  await page.goto('/ground-leases')
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseId } = await post<{ groundLeaseId: string }>(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    leaseStartDate: '2020-01-01',
    termYears: 99,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'IndexBased',
    indexType: 'CPI',
    baselineIndexValue: 100,
  })

  await page.reload()
  await leaseRowByHome(page, homeId).click()
  const detail = panel(page, 'Ground Lease')
  await expect(detailValue(detail, 'Index Type')).toHaveText('CPI')
  await expect(detailValue(detail, 'Baseline Index')).toHaveText('100')
  await detail.getByRole('button', { name: 'Start Resale Workflow' }).click()

  const resale = panel(page, `Resale — ${groundLeaseId.slice(0, 8)}…`)
  await expect(detailValue(resale, 'Formula')).toHaveText('Index Based')
  await expect(detailValue(resale, 'Ground Lease ID')).toHaveText(groundLeaseId)
  await resale.getByRole('button', { name: 'Calculate Resale Price →' }).click()

  await field(resale, 'Original Purchase Price ($)').fill('200000')
  await field(resale, 'Current Appraisal Value ($)').fill('260000')
  await field(resale, 'Original Appraisal Value ($)').fill('210000')
  await field(resale, 'Ownership Years').fill('4')
  await resale.getByRole('button', { name: 'Initiate Purchase Workflow →' }).click()
  // Index-based leases need the current index value.
  await expect(resale.locator('.form-error')).toHaveText(
    'Current index value is required to estimate an index-based resale price.')
  await expect(resale.locator('.slideover-header')).toContainText('Step 2 of 3: Estimate')

  await field(resale, 'Current Index Value').fill('150')
  await resale.getByRole('button', { name: 'Initiate Purchase Workflow →' }).click()
  await expect(resale.locator('.slideover-header')).toContainText('Step 3 of 3: Initiate')
  await expect(field(resale, 'Purchase Price ($)')).toHaveValue('300000')
  await expect(resale.locator('.form-error')).toHaveText(
    'This ground lease has no lease template, so a purchase workflow cannot be initiated from it.')
  await expect(resale.getByRole('button', { name: 'Initiate Workflow' })).toBeDisabled()
})
