import { expect, test, type Page } from '@playwright/test'

// Journeys (docs/journeys/navigation/):
//   choose-a-focus — picking a focus splits Work into My work and Other work and is remembered

const sidebar = (page: Page) => page.locator('.sidebar')
const navLink = (page: Page, name: string) => sidebar(page).getByRole('link', { name, exact: true })

test('choosing a focus puts its pages under My work and is remembered', async ({ page }) => {
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()
  await expect(sidebar(page).getByText('Work', { exact: true })).toBeVisible()
  await expect(navLink(page, 'Stewardship')).toBeVisible()

  await page.getByRole('combobox', { name: 'My focus' }).selectOption('Sales')

  // Sales pages under My work; the rest folded away under Other work.
  await expect(sidebar(page).getByText('My work', { exact: true })).toBeVisible()
  await expect(navLink(page, 'Home selections')).toBeVisible()
  await expect(navLink(page, 'Purchases')).toBeVisible()
  await expect(navLink(page, 'Households')).toBeHidden()
  // Records and Settings are unaffected.
  await expect(navLink(page, 'Land parcels')).toBeVisible()
  await expect(navLink(page, 'Lease templates')).toBeVisible()

  const otherWork = sidebar(page).getByRole('button', { name: 'Other work' })
  await otherWork.click()
  await expect(otherWork).toHaveAttribute('aria-expanded', 'true')
  await expect(navLink(page, 'Households')).toBeVisible()
  await navLink(page, 'Households').click()
  await expect(page).toHaveURL(/\/households$/)

  // The focus and the open toggle survive a reload; the root lands on Today.
  await page.goto('/')
  await expect(page.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()
  await expect(page.getByRole('combobox', { name: 'My focus' })).toHaveValue('sales')
  await expect(navLink(page, 'Households')).toBeVisible()

  await otherWork.click()
  await expect(navLink(page, 'Households')).toBeHidden()

  await page.getByRole('combobox', { name: 'My focus' }).selectOption('Everything')
  await expect(sidebar(page).getByText('Work', { exact: true })).toBeVisible()
  await expect(otherWork).toBeHidden()
  await expect(navLink(page, 'Households')).toBeVisible()
})
