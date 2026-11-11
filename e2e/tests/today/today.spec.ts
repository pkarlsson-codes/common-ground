import { expect, test, type Page } from '@playwright/test'
import { createApplication, createHome, createLandParcel, createPurchaseWorkflow, post, setDocumentExpiry, uploadDocument } from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys (docs/journeys/today/):
//   work-through-today — items grouped by area, a row's actions fix it on Today, a row opens its record, focus narrows the areas

const section = (page: Page, name: string) => page.getByRole('region', { name: new RegExp(`^${name}`) })

test('a submitted application waits on Today until it is approved there, without leaving', async ({ page }) => {
  const name = `Today ${uniqueId()}`
  await createApplication(page, name, { approve: false })

  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()
  await expect(page.locator('.page-header__count')).toContainText('need')

  const row = section(page, 'Applicants').getByRole('row', { name: new RegExp(`Application to decide ${name}`) })
  await expect(row).toContainText('Submitted, waiting for a decision')
  await expect(row).toContainText(/Waiting \d+ day|Since today/)
  await expect(row.getByRole('button', { name: `Deny: ${name}` })).toBeVisible()

  await row.getByRole('button', { name: `Approve: ${name}` }).click()
  await panel(page, 'Approve Application').getByRole('button', { name: 'Approve', exact: true }).click()
  await expectToast(page, 'Application approved')

  await expect(page).toHaveURL(/\/$/)
  await expect(page.getByRole('row', { name: new RegExp(`Application to decide ${name}`) })).toHaveCount(0)
})

test('a household missing a readiness step offers to record just that step, on Today', async ({ page }) => {
  const name = `Today ready ${uniqueId()}`
  await createApplication(page, name, { ready: false })

  await page.goto('/')
  const row = section(page, 'Applicants').getByRole('row', { name: new RegExp(`Readiness incomplete ${name}`) })
  await expect(row).toContainText('Missing CLT orientation')
  await expect(row.getByRole('button', { name: `Record counseling: ${name}` })).toHaveCount(0)

  await row.getByRole('button', { name: `Record orientation: ${name}` }).click()
  await panel(page, 'Record CLT Orientation').getByRole('button', { name: 'Record Orientation' }).click()
  await expectToast(page, 'CLT orientation recorded')

  await expect(page).toHaveURL(/\/$/)
  await expect(page.getByRole('row', { name: new RegExp(`Readiness incomplete ${name}`) })).toHaveCount(0)
})

test('an action with no panel on Today yet opens the record, where the action is', async ({ page }) => {
  const { parcelId, apn } = await createLandParcel(page)
  await createHome(page, parcelId)

  await page.goto('/')
  const row = section(page, 'Selections').getByRole('row', { name: new RegExp(`Home available ${apn}`) })
  await row.getByRole('button', { name: /^Start selection: / }).click()

  await expect(page).toHaveURL(/\/homes$/)
  await expect(panel(page, `${apn} Elm St`).getByRole('button', { name: 'Start Selection' })).toBeVisible()
})

test('a document expiring soon is a Records item with a due chip, and opens the document', async ({ page }) => {
  const title = `Today doc ${uniqueId()}`
  const { documentId } = await uploadDocument(page, title)
  await setDocumentExpiry(page, documentId, isoDate(3))

  await page.goto('/')
  const row = section(page, 'Records').getByRole('row', { name: new RegExp(title) })
  await expect(row.locator('.due-chip--soon')).toHaveText(/^Due /)

  await row.click()
  await expect(page).toHaveURL(/\/documents$/)
  await expect(panel(page, title)).toBeVisible()
})

test('with a focus, Today shows only its areas plus Records until All areas is chosen', async ({ page }) => {
  const name = `Today focus ${uniqueId()}`
  await createApplication(page, name, { approve: false })

  await page.goto('/')
  await page.getByRole('combobox', { name: 'My focus' }).selectOption('Sales')

  await expect(section(page, 'Applicants')).toHaveCount(0)
  await expect(page.locator('.today-hidden-note')).toContainText('in other areas')
  await expect(page.getByRole('button', { name: 'My areas' })).toHaveAttribute('aria-pressed', 'true')

  // The sidebar's Today badge counts what My areas shows; the collapsed Other work toggle counts the pages inside it.
  const todayBadge = page.locator('.sidebar').getByRole('link', { name: 'Today', exact: true }).locator('.nav-item__count')
  const rows = page.locator('main tbody tr')
  await expect.poll(async () => Number(await todayBadge.textContent()) === await rows.count()).toBe(true)
  const otherWorkBadge = page.locator('.sidebar').getByRole('button', { name: 'Other work' }).locator('.nav-item__count')
  await expect(otherWorkBadge).toBeVisible()

  await page.getByRole('button', { name: 'All areas' }).click()
  await expect(section(page, 'Applicants').getByRole('row', { name: new RegExp(name) })).toBeVisible()
  await expect(page.locator('.today-hidden-note')).toBeHidden()
})

test('a purchase with eligibility recorded waits on its appraisal, and Today opens the purchase to record it', async ({ page }) => {
  const buyer = `Today appraisal ${uniqueId()}`
  const { workflowId, apn } = await createPurchaseWorkflow(page, buyer)
  await post(page, `/api/v1/purchase-workflows/${workflowId}/eligibility`, { verifiedDate: isoDate(), notes: null })

  // Today reads the purchase view, which catches up with the recorded eligibility after the POST returns.
  const row = section(page, 'Purchases').getByRole('row', { name: new RegExp(apn) })
  await expect(async () => {
    await page.goto('/')
    await expect(row).toContainText(`${buyer}: waiting on appraisal`, { timeout: 2_000 })
  }).toPass()
  await expect(row.getByRole('button', { name: new RegExp(`^Record appraisal: `) })).toBeVisible()
  await expect(row.getByRole('button', { name: /^Record eligibility: / })).toHaveCount(0)

  await row.getByRole('button', { name: /^Record appraisal: / }).click()
  await expect(page).toHaveURL(/\/purchase-workflows$/)
  const detail = panel(page, buyer)
  await detail.getByText(/^Appraisal$/).locator('xpath=../..').getByRole('button', { name: 'Expand' }).click()
  await field(detail, 'Appraiser Name').fill('Today Appraiser')
  await field(detail, 'Appraised Value ($)').fill('240000')
  await detail.getByRole('button', { name: 'Record Appraisal' }).click()
  await expectToast(page, 'Appraisal recorded')
})
