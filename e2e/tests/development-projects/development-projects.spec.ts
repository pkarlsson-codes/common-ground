import { expect, test, type Page } from '@playwright/test'
import { createHome, createLandParcel, post } from '../support/api'
import { detailValue, expectToast, field, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/development-projects/
//   initiate-development-project, view-development-project-detail

async function searchProjects(page: Page, term: string, count = 1) {
  await page.getByRole('searchbox', { name: 'Search development projects' }).fill(term)
  await expect(page.locator('tbody tr')).toHaveCount(count)
  return page.locator('tbody tr')
}

async function pickParcel(page: Page, apn: string) {
  await field(panel(page, 'Initiate Development Project'), 'Land Parcel').click()
  await page.getByPlaceholder('Search parcels…').fill(apn)
  await page.getByRole('listbox').getByRole('option', { name: `${apn} — ${apn} Elm St` }).click()
}

test('initiating a new construction project lists it and opens its detail', async ({ page }) => {
  await page.goto('/development')
  const { parcelId, apn } = await createLandParcel(page)

  await page.getByRole('button', { name: 'Initiate Project' }).click()
  const form = panel(page, 'Initiate Development Project')
  const initiate = form.getByRole('button', { name: 'Initiate Project' })
  await expect(form.getByRole('button', { name: 'New Construction' })).toHaveClass(/btn-primary/)
  await expect(form.locator('.form-label', { hasText: /^Homes$/ })).toHaveCount(0)
  await expect(initiate).toBeDisabled()

  await pickParcel(page, apn)
  const name = `Infill ${uniqueId()}`
  await field(form, 'Name').fill(name)
  await expect(initiate).toBeDisabled()
  await field(form, 'Description').fill('Four new cottages')
  await initiate.click()
  await expectToast(page, 'Development project initiated')
  await expect(form).toBeHidden()

  const row = await searchProjects(page, name.toUpperCase())
  await expect(row.locator(`[title="${parcelId}"]`)).toBeVisible()
  await expect(row).toContainText('New Construction')
  await expect(row).toContainText('Initiated')
  await row.click()

  const detail = panel(page, name)
  await expect(detail.locator('.slideover-header')).toContainText('New Construction · Initiated')
  await expect(detailValue(detail, 'Type')).toHaveText('New Construction')
  await expect(detailValue(detail, 'Description')).toHaveText('Four new cottages')
  await expect(detail.locator('.detail-field__label', { hasText: /^Homes$/ })).toHaveCount(0)

  await detail.getByRole('button', { name: 'Open Land Parcel' }).click()
  await expect(page).toHaveURL(/\/land-parcels$/)
  await expect(panel(page, apn)).toBeVisible()
})

test('initiating a rehabilitation project requires a home on the parcel and links to it', async ({ page }) => {
  await page.goto('/development')
  const { parcelId, apn } = await createLandParcel(page)
  await createHome(page, parcelId)

  await page.getByRole('button', { name: 'Initiate Project' }).click()
  const form = panel(page, 'Initiate Development Project')
  const initiate = form.getByRole('button', { name: 'Initiate Project' })
  await form.getByRole('button', { name: 'Rehabilitation' }).click()
  await expect(form).toContainText('Select a parcel to load homes.')

  await pickParcel(page, apn)
  const name = `Rehab ${uniqueId()}`
  await field(form, 'Name').fill(name)
  await field(form, 'Description').fill('Replace windows and siding')
  await expect(initiate).toBeDisabled()

  const homes = field(form, 'Homes')
  await homes.click()
  await page.getByRole('option', { name: 'Single Family — 3bd / 2ba' }).click()
  await homes.click()
  await expect(homes).toContainText('Single Family — 3bd / 2ba')
  await expect(initiate).toBeEnabled()

  // Switching type clears the selected homes.
  await form.getByRole('button', { name: 'New Construction' }).click()
  await form.getByRole('button', { name: 'Rehabilitation' }).click()
  await expect(homes).toContainText('Select homes…')
  await expect(initiate).toBeDisabled()
  await homes.click()
  await page.getByRole('option', { name: 'Single Family — 3bd / 2ba' }).click()
  await homes.click()

  await initiate.click()
  await expectToast(page, 'Development project initiated')
  await expect(form).toBeHidden()

  const row = await searchProjects(page, name)
  await expect(row).toContainText('Rehabilitation')
  await row.click()

  const detail = panel(page, name)
  await expect(detail.locator('.slideover-header')).toContainText('Rehabilitation · Initiated')
  await expect(detailValue(detail, 'Homes')).toContainText('Single Family · 3 bd / 2.0 ba')
  await detail.getByRole('button', { name: 'Open Home' }).click()
  await expect(page).toHaveURL(/\/homes$/)
  await expect(panel(page, `${apn} Elm St`)).toBeVisible()
})

test('projects list newest first and search matches name or description', async ({ page }) => {
  await page.goto('/development')
  const { parcelId } = await createLandParcel(page)
  const token = uniqueId()
  const initiate = (name: string, description: string) =>
    post(page, '/api/v1/development-projects', { landParcelId: parcelId, name, type: 'NewConstruction', description })
  await initiate(`Older ${token}`, 'Site prep')
  await initiate('Newer project', `Scope ${token}`)

  const rows = await searchProjects(page, token, 2)
  await expect(rows.nth(0)).toContainText('Newer project')
  await expect(rows.nth(1)).toContainText(`Older ${token}`)
})
