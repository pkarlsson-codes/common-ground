import { expect, test } from '@playwright/test'
import { createApplication, createHome, createLandParcel, post } from '../support/api'
import { uniqueId } from '../support/ui'

// Journeys (docs/journeys/home-selections/):
//   view-home-selections — selections across homes, open ones by default, filtered by status
//   start-home-selection, view-home-selection — candidates judged against the home's income limit

test('the Home selections page lists open selections across homes and opens one', async ({ page }) => {
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  await createApplication(page, `Waiting ${uniqueId()}`)
  // HomeBuyers learns about the new home from an integration event, so retry until it has.
  let selectionId = ''
  await expect(async () => {
    ;({ selectionId } = await post<{ selectionId: string }>(page, `/api/v1/homes/${homeId}/home-selections`, {}))
  }).toPass()

  await page.goto('/home-selections')
  await expect(page.getByRole('heading', { name: 'Home Selections', level: 1 })).toBeVisible()
  await expect(page.getByRole('combobox', { name: 'Filter by status' })).toHaveValue('Open')
  const row = page.getByRole('row', { name: new RegExp(`${apn} Elm St`) })
  await expect(row).toContainText('Open')

  await page.getByRole('combobox', { name: 'Filter by status' }).selectOption('Cancelled')
  await expect(row).toHaveCount(0)
  await page.getByRole('combobox', { name: 'Filter by status' }).selectOption('Open')

  await row.click()
  await expect(page).toHaveURL(new RegExp(`/homes/selections/${selectionId}$`))
  await expect(page.getByRole('heading', { name: `${apn} Elm St`, level: 1 })).toBeVisible()
})

test('a household over the home income limit is marked and cannot be selected', async ({ page }) => {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const under = await createApplication(page, `Under ${uniqueId()}`)
  const over = await createApplication(page, `Over ${uniqueId()}`, { annualIncome: 90000 })
  let selectionId = ''
  await expect(async () => {
    ;({ selectionId } = await post<{ selectionId: string }>(page, `/api/v1/homes/${homeId}/home-selections`, {}))
  }).toPass()

  await page.goto(`/homes/selections/${selectionId}`)
  await expect(page.locator('.page-header__count')).toContainText('80% of AMI, limits effective Jan 1, 2000')

  const overRow = page.getByRole('row', { name: new RegExp(over.name) })
  await expect(overRow).toContainText('$70,000')
  await expect(overRow).toContainText('Over income limit')
  await expect(overRow).toContainText('No (over income limit)')
  await expect(overRow.getByRole('button', { name: 'Select' })).toBeDisabled()

  const underRow = page.getByRole('row', { name: new RegExp(under.name) })
  await expect(underRow).toContainText('$70,000')
  await expect(underRow).not.toContainText('Over income limit')
  await expect(underRow.getByRole('button', { name: 'Select' })).toBeEnabled()
})
