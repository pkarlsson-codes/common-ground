import { expect, test, type Page } from '@playwright/test'
import { createApplication, setDocumentExpiry, uploadDocument } from '../support/api'
import { expectToast, isoDate, panel, uniqueId } from '../support/ui'

// Journeys (docs/journeys/today/):
//   work-through-today — items grouped by area, a row opens its record, acting on it takes it off, focus narrows the areas

const section = (page: Page, name: string) => page.getByRole('region', { name: new RegExp(`^${name}`) })

test('a submitted application waits on Today until it is approved from there', async ({ page }) => {
  const name = `Today ${uniqueId()}`
  await createApplication(page, name, { approve: false })

  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()
  await expect(page.locator('.page-header__count')).toContainText('need')

  const row = section(page, 'Applicants').getByRole('row', { name: new RegExp(`Application to decide ${name}`) })
  await expect(row).toContainText('Submitted, waiting for a decision')
  await expect(row).toContainText(/Waiting \d+ day|Since today/)

  await row.click()
  await expect(page).toHaveURL(/\/applications$/)
  const detail = panel(page, name)
  await detail.getByRole('button', { name: 'Approve Application' }).click()
  await panel(page, 'Approve Application').getByRole('button', { name: 'Approve', exact: true }).click()
  await expectToast(page, 'Application approved')
  await detail.getByRole('button', { name: 'Close' }).first().click()

  await page.locator('.sidebar').getByRole('link', { name: 'Today', exact: true }).click()
  await expect(page).toHaveURL(/\/$/)
  await expect(page.getByRole('row', { name: new RegExp(name) })).toHaveCount(0)
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

  await page.getByRole('button', { name: 'All areas' }).click()
  await expect(section(page, 'Applicants').getByRole('row', { name: new RegExp(name) })).toBeVisible()
  await expect(page.locator('.today-hidden-note')).toBeHidden()
})
