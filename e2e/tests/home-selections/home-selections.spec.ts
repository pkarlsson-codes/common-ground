import { expect, test } from '@playwright/test'
import { createApplication, createHome, createLandParcel, post } from '../support/api'
import { uniqueId } from '../support/ui'

// Journeys (docs/journeys/home-selections/):
//   view-home-selections — selections across homes, open ones by default, filtered by status

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
