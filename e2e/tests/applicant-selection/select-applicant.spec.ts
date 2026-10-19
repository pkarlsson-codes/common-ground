import { expect, test } from '@playwright/test'
import { createApplication, createGroundLeaseTemplate, createHome, createLandParcel, get } from '../support/api'
import { detailValue, expectToast, field, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/applicant-selection/select-applicant.md, view-application-detail.md

test('selecting an approved applicant puts the home under contract and starts a purchase workflow', async ({ page }) => {
  await page.goto('/applicant-selection')

  const id = uniqueId()
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseTemplateId, templateName } = await createGroundLeaseTemplate(page)
  const approved = await createApplication(page, `Approved ${id}`)
  const submitted = await createApplication(page, `Submitted ${id}`, { approve: false })

  // Open the home via the parcel filter (the parcel is unique to this test).
  await page.goto('/homes')
  await page.getByRole('combobox').filter({ hasText: 'Filter by parcel…' }).click()
  await page.getByPlaceholder('Filter by parcel…').fill(apn)
  await page.getByRole('listbox').getByRole('option', { name: new RegExp(apn) }).click()
  await expect(page.locator('tbody tr')).toHaveCount(1)
  await page.locator('tbody tr').first().click()

  await page.getByRole('button', { name: 'Select Applicant' }).click()
  const select = panel(page, 'Select Applicant')

  // Only Approved applications are offered; typing filters locally.
  await field(select, 'Applicant').click()
  const search = page.getByPlaceholder('Search approved applicants…')
  await search.fill(id)
  await expect(page.getByRole('listbox').getByRole('option')).toHaveText([`${approved.name} — ${approved.email}`])
  await expect(page.getByRole('listbox').getByRole('option', { name: new RegExp(submitted.name) })).toHaveCount(0)
  await page.getByRole('listbox').getByRole('option', { name: new RegExp(approved.name) }).click()

  const submit = select.getByRole('button', { name: 'Select Applicant' })
  await expect(submit).toBeDisabled()

  await field(select, 'Ground Lease Template').click()
  await page.getByPlaceholder('Search templates…').fill(templateName)
  await page.getByRole('listbox').getByRole('option', { name: templateName }).click()
  await field(select, 'Purchase Price ($)').fill('200000')

  await expect(submit).toBeEnabled()
  await submit.click()
  await expectToast(page, 'Applicant selected')
  await expect(select).toBeHidden()

  // Home status and the purchase workflow are updated by integration events, so poll.
  await expect.poll(async () => (await get<{ status: string }>(page, `/api/v1/homes/${homeId}`)).status)
    .toBe('UnderContract')
  await expect.poll(async () => {
    const workflows = await get<{ items: { buyerFullName: string }[] }>(
      page, `/api/v1/purchase-workflows?groundLeaseTemplateId=${groundLeaseTemplateId}`)
    return workflows.items.map(w => w.buyerFullName)
  }).toEqual([approved.name])

  // The application shows as Selected, linked back to the home.
  await page.goto('/applicant-selection')
  await page.getByRole('row', { name: new RegExp(approved.name) }).click()
  const detail = panel(page, approved.name)
  await expect(detailValue(detail, 'Phase')).toHaveText('Selected')
  await expect(detail.getByText('No actions available for this phase.')).toBeVisible()
  await expect(detail.getByText('Open Selected Home')).toBeVisible()
})
