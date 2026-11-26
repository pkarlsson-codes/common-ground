import { expect, test, type Page } from '@playwright/test'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys (docs/journeys/reminders/):
//   set-a-reminder — New reminder on Today, in the header or a section, sets a dated note in a section or none
//   mark-a-reminder-done — Mark done on its Today row takes it off Today
//   view-reminders — the Reminders page lists reminders on Today, upcoming, and done lately

const section = (page: Page, name: string) => page.getByRole('region', { name: new RegExp(`^${name}`) })

async function setReminder(page: Page, opener: string, note: string, offsetDays: number, sectionLabel?: string) {
  await page.getByRole('button', { name: opener, exact: true }).click()
  const form = panel(page, 'New reminder')
  await field(form, 'Date').fill(isoDate(offsetDays))
  if (sectionLabel) await field(form, 'Section').selectOption(sectionLabel)
  await field(form, 'Note').fill(note)
  await form.getByRole('button', { name: 'Set reminder' }).click()
  await expectToast(page, 'Reminder set')
}

test('a reminder set in Stewardship shows there on its date whatever the focus, until it is marked done', async ({ page }) => {
  const today = `Call the lender about the rate lock ${uniqueId()}`
  const later = `Bring the default to the board ${uniqueId()}`

  await page.goto('/')
  await page.getByRole('combobox', { name: 'My focus' }).selectOption('Intake')
  await expect(page.getByRole('button', { name: 'My areas' })).toHaveAttribute('aria-pressed', 'true')

  // Set for today from the header: it shows in Stewardship although the focus leaves Stewardship out.
  await setReminder(page, 'New reminder', today, 0, 'Stewardship')
  const row = section(page, 'Stewardship').getByRole('row', { name: new RegExp(today) })
  await expect(row).toContainText('Reminder')
  await expect(row.locator('.due-chip')).toHaveText(/^Set for /)

  // The section's own button preselects its section; a later date keeps it off Today.
  await page.getByRole('button', { name: 'New reminder in Stewardship' }).click()
  await expect(field(panel(page, 'New reminder'), 'Section')).toHaveValue('stewardship')
  await page.getByRole('button', { name: 'Cancel' }).click()
  await setReminder(page, 'New reminder in Stewardship', later, 3)
  await expect(page.getByRole('row', { name: new RegExp(later) })).toHaveCount(0)

  // The Reminders page, just below Today, lists both.
  await page.locator('.sidebar').getByRole('link', { name: 'Reminders', exact: true }).click()
  await expect(page.getByRole('heading', { name: 'Reminders', level: 1 })).toBeVisible()
  await expect(section(page, 'On Today').getByRole('row', { name: new RegExp(today) })).toContainText('Stewardship')
  await expect(section(page, 'Upcoming').getByRole('row', { name: new RegExp(later) })).toContainText('Stewardship')

  // Marked done on Today, it leaves Today and is listed as done.
  await page.locator('.sidebar').getByRole('link', { name: 'Today', exact: true }).click()
  await section(page, 'Stewardship').getByRole('button', { name: `Mark done: ${today}` }).click()
  await expectToast(page, 'Reminder marked done')
  await expect(page.getByRole('row', { name: new RegExp(today) })).toHaveCount(0)

  await page.locator('.sidebar').getByRole('link', { name: 'Reminders', exact: true }).click()
  await expect(section(page, 'Done in the last 30 days').getByRole('row', { name: new RegExp(today) })).toBeVisible()
  await expect(section(page, 'On Today').getByRole('row', { name: new RegExp(today) })).toHaveCount(0)
})

test('a reminder set in no section shows in the Reminders section on Today', async ({ page }) => {
  const note = `Order more lease binders ${uniqueId()}`

  await page.goto('/reminders')
  await setReminder(page, 'New reminder', note, 0)

  await page.locator('.sidebar').getByRole('link', { name: 'Today', exact: true }).click()
  await expect(section(page, 'Reminders').getByRole('row', { name: new RegExp(note) })).toBeVisible()
})
