import { expect, test, type Locator, type Page } from '@playwright/test'
import { createLandParcel } from '../support/api'
import { detailValue, expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/land-parcels/
//   register-land-parcel, view-land-parcel-detail, decommission-land-parcel, recommission-land-parcel,
//   register-home-from-parcel, initiate-development-project-from-parcel

/** Narrows the Land Parcels table to one parcel by searching for its (unique) parcel number. */
async function searchParcels(page: Page, apn: string) {
  await page.getByRole('searchbox', { name: 'Search land parcels' }).fill(apn)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  return page.getByRole('row', { name: new RegExp(apn) })
}

async function openParcel(page: Page, apn: string) {
  await page.goto('/land-parcels')
  await (await searchParcels(page, apn)).click()
  return panel(page, apn)
}

/** A collapsible section of the parcel detail panel ("Homes" or "Development Projects"). */
const section = (detail: Locator, title: string) =>
  detail.locator('.detail-section').filter({ has: detail.page().getByRole('button', { name: new RegExp(`^[▾▸] ${title}`) }) })

test('registering a parcel adds it to the registry with the entered details', async ({ page }) => {
  await page.goto('/land-parcels')
  const apn = `APN-${uniqueId()}`
  const address = `${apn} Registry Rd`

  await page.getByRole('button', { name: 'Register Parcel' }).click()
  const register = panel(page, 'Register Land Parcel')
  await field(register, 'Legal Parcel Number').fill(apn)
  await field(register, 'Address').fill(address)
  await field(register, 'Area (acres)').fill('1.2345')
  await field(register, 'Acquisition Date').fill('2024-03-15')
  await field(register, 'Acquisition Method').selectOption('Donation')
  await field(register, 'Covenant Terms (optional)').fill('Permanently affordable')
  await register.getByRole('button', { name: 'Register Parcel' }).click()
  await expectToast(page, 'Land parcel registered successfully')
  await expect(register).toBeHidden()

  const row = await searchParcels(page, apn)
  await expect(row).toContainText(address)
  await expect(row).toContainText('1.23')
  await expect(row).toContainText('Active')

  // Searching by address finds it too.
  await page.getByRole('searchbox', { name: 'Search land parcels' }).fill(`${apn} Registry`)
  await expect(page.locator('tbody tr')).toHaveCount(1)

  await row.click()
  const detail = panel(page, apn)
  await expect(detail).toContainText(address)
  await expect(detailValue(detail, 'Status')).toHaveText('Active')
  await expect(detailValue(detail, 'Area')).toHaveText('1.2345 ac')
  await expect(detailValue(detail, 'Acquisition Method')).toHaveText('Donation')
  await expect(detailValue(detail, 'Acquisition Date')).toHaveText('2024-03-15')
  await expect(detailValue(detail, 'Registered By')).not.toBeEmpty()
  await expect(detailValue(detail, 'Covenant Terms')).toHaveText('Permanently affordable')
  await expect(detailValue(detail, 'Registered At')).not.toBeEmpty()

  const homes = section(detail, 'Homes')
  await expect(homes).toContainText('Homes (0)')
  await expect(homes).toContainText('No homes registered on this parcel.')
  const projects = section(detail, 'Development Projects')
  await expect(projects).toContainText('Development Projects (0)')
  await expect(projects).toContainText('No development projects on this parcel.')

  // The sections collapse and expand independently.
  await homes.getByRole('button', { name: /Homes/ }).click()
  await expect(homes).not.toContainText('No homes registered on this parcel.')
  await expect(projects).toContainText('No development projects on this parcel.')
  await homes.getByRole('button', { name: /Homes/ }).click()
  await expect(homes).toContainText('No homes registered on this parcel.')

  await page.keyboard.press('Escape')
  await expect(detail).toBeHidden()
})

test('a parcel registered without covenant terms shows no covenant field', async ({ page }) => {
  await page.goto('/land-parcels')
  const { apn } = await createLandParcel(page)

  const detail = await openParcel(page, apn)
  await expect(detailValue(detail, 'Acquisition Method')).toHaveText('Purchase')
  await expect(detail.locator('.detail-field__label', { hasText: 'Covenant Terms' })).toHaveCount(0)
})

test('decommissioning and recommissioning a parcel asks for confirmation and updates its status', async ({ page }) => {
  await page.goto('/land-parcels')
  const { apn } = await createLandParcel(page)
  const detail = await openParcel(page, apn)

  await detail.getByRole('button', { name: 'Actions' }).click()
  await expect(detail.getByRole('button', { name: 'Recommission' })).toHaveCount(0)
  await detail.getByRole('button', { name: 'Decommission' }).click()

  // Cancelling leaves the parcel active.
  await expect(detail).toContainText('Decommission this parcel? It will be marked inactive.')
  await detail.getByRole('button', { name: 'Cancel' }).click()
  await expect(detail).not.toContainText('Decommission this parcel?')
  await expect(detailValue(detail, 'Status')).toHaveText('Active')

  await detail.getByRole('button', { name: 'Actions' }).click()
  await detail.getByRole('button', { name: 'Decommission' }).click()
  await detail.getByRole('button', { name: 'Confirm' }).click()
  await expectToast(page, 'Parcel decommissioned')
  await expect(detailValue(detail, 'Status')).toHaveText('Decommissioned')

  // Decommissioned parcels drop out of the table unless "Show decommissioned" is checked.
  await expect(page.locator('.empty-state')).toBeVisible()
  await page.keyboard.press('Escape')
  await expect(detail).toBeHidden()
  await page.getByRole('checkbox', { name: 'Show decommissioned' }).check()
  await expect(page.getByRole('row', { name: new RegExp(apn) })).toContainText('Decommissioned')
  await page.getByRole('row', { name: new RegExp(apn) }).click()

  await detail.getByRole('button', { name: 'Actions' }).click()
  await expect(detail.getByRole('button', { name: 'Decommission' })).toHaveCount(0)
  await detail.getByRole('button', { name: 'Recommission' }).click()
  await expect(detail).toContainText('Recommission this parcel? It will be marked active again.')
  await detail.getByRole('button', { name: 'Confirm' }).click()
  await expectToast(page, 'Parcel recommissioned')
  await expect(detailValue(detail, 'Status')).toHaveText('Active')
  await expect(page.getByRole('row', { name: new RegExp(apn) })).toContainText('Active')
})

test('registering a home from a parcel locks the parcel and lists the home on it', async ({ page }) => {
  await page.goto('/land-parcels')
  const { apn } = await createLandParcel(page)
  const detail = await openParcel(page, apn)

  await section(detail, 'Homes').getByRole('button', { name: '+ Register Home' }).click()
  const register = panel(page, 'Register Home')
  // The parcel is shown as read-only text rather than a combobox.
  await expect(register.locator('.form-field').first()).toContainText(`${apn} — ${apn} Elm St`)
  await expect(register.locator('.form-field').first().getByRole('combobox')).toHaveCount(0)

  await field(register, 'Home Type').selectOption('Townhouse')
  await field(register, 'Bedrooms').fill('2')
  await field(register, 'Bathrooms').fill('1.5')
  await field(register, 'Square Footage').fill('950')
  await field(register, 'Year Built').fill('2005')
  await field(register, 'Initial Assessed Value ($)').fill('240000')
  await field(register, 'Assessment Date').fill(isoDate(-30))
  await register.getByRole('button', { name: 'Register Home' }).click()
  await expectToast(page, 'Home registered successfully')
  await expect(register).toBeHidden()

  const homes = section(detail, 'Homes')
  await expect(homes).toContainText('Homes (1)')
  await expect(homes).toContainText('Townhouse · 2 bd / 1.5 ba')
  await expect(homes).toContainText('Available')
})

test('initiating a development project from a parcel locks the parcel and lists the project on it', async ({ page }) => {
  await page.goto('/land-parcels')
  const { apn } = await createLandParcel(page)
  const detail = await openParcel(page, apn)

  await section(detail, 'Development Projects').getByRole('button', { name: '+ Initiate Project' }).click()
  const initiate = panel(page, 'Initiate Development Project')
  await expect(field(initiate, 'Land Parcel')).toBeDisabled()
  await expect(field(initiate, 'Land Parcel')).toHaveText(`${apn} — ${apn} Elm St`)
  await expect(initiate.getByRole('button', { name: 'Initiate Project' })).toBeDisabled()

  const name = `Project ${uniqueId()}`
  await field(initiate, 'Name').fill(name)
  await expect(initiate.getByRole('button', { name: 'Initiate Project' })).toBeDisabled()
  await field(initiate, 'Description').fill('Four new cottages')
  await initiate.getByRole('button', { name: 'Initiate Project' }).click()
  await expectToast(page, 'Development project initiated')
  await expect(initiate).toBeHidden()

  const projects = section(detail, 'Development Projects')
  await expect(projects).toContainText('Development Projects (1)')
  await expect(projects).toContainText(name)
})
