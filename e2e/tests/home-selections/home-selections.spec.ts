import { expect, test } from '@playwright/test'
import { createApplication, createGroundLeaseTemplate, createHome, createLandParcel, post, waitForHomeBuyersToKnowHome } from '../support/api'
import { isoDate, uniqueId } from '../support/ui'

// Journeys (docs/journeys/home-selections/):
//   view-home-selections — the households chosen across homes, newest first
//   view-home-selection — the waiting list compared against the home, and the households chosen for it
//   select-household-by-priority — one step, recorded with the whole waiting list

test('the Home selections page lists the households chosen across homes and opens the home', async ({ page }) => {
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const chosen = await createApplication(page, `Chosen ${uniqueId()}`)
  const { groundLeaseTemplateId } = await createGroundLeaseTemplate(page)
  await waitForHomeBuyersToKnowHome(page, homeId)
  await post(page, `/api/v1/homes/${homeId}/home-selections/select-by-priority`, {
    applicationId: chosen.applicationId,
    priorities: ['WaitingListTenure'],
    reason: 'Longest waiting',
    groundLeaseTemplateId,
    purchasePrice: 200000,
    selectedDate: isoDate(),
  })

  await page.goto('/home-selections')
  await expect(page.getByRole('heading', { name: 'Home Selections', level: 1 })).toBeVisible()
  const row = page.getByRole('row', { name: new RegExp(`${apn} Elm St`) })
  await expect(row).toContainText(chosen.name)
  await expect(row).toContainText('Priority')

  await row.click()
  await expect(page).toHaveURL(new RegExp(`/homes/${homeId}/selection$`))
  await expect(page.getByRole('heading', { name: `${apn} Elm St`, level: 1 })).toBeVisible()
  await expect(page.getByText(`${chosen.name} chosen`)).toBeVisible()
  await expect(page.getByText('A household has been chosen for this home.')).toBeVisible()
  await expect(page.getByRole('button', { name: 'Choose' })).toHaveCount(0)
})

test('an available home compares the waiting list against it, with no Start button, and a household over its income limit cannot be chosen', async ({ page }) => {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const under = await createApplication(page, `Under ${uniqueId()}`)
  const over = await createApplication(page, `Over ${uniqueId()}`, { annualIncome: 90000 })
  await waitForHomeBuyersToKnowHome(page, homeId)

  await page.goto(`/homes/${homeId}/selection`)
  await expect(page.locator('.page-header__count')).toContainText('80% of AMI, limits effective Jan 1, 2000')
  await expect(page.getByRole('button', { name: /Start/ })).toHaveCount(0)

  const overRow = page.getByRole('row', { name: new RegExp(over.name) })
  await expect(overRow).toContainText('$70,000')
  await expect(overRow).toContainText('Over income limit')
  await expect(overRow.getByRole('button', { name: 'Choose' })).toBeDisabled()

  const underRow = page.getByRole('row', { name: new RegExp(under.name) })
  await expect(underRow).toContainText('$70,000')
  await expect(underRow).not.toContainText('Over income limit')
  await expect(underRow.getByRole('button', { name: 'Choose' })).toBeEnabled()
})
