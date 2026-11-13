import { expect, test, type Locator } from '@playwright/test'
import { expectToast, field, panel } from '../support/ui'

// Journeys (docs/journeys/income-limits/):
//   enter-income-limits — list, enter (by pasting a HUD row) and correct a schedule

/**
 * A date far in the future that no other run has used. Entering a schedule dated between the baseline and today
 * would replace the limits every other test judges by, so this one never takes effect.
 */
const futureEffectiveDate = () => {
  const d = new Date(Date.UTC(2100, 0, 1))
  d.setUTCDate(d.getUTCDate() + Math.floor(Math.random() * 800 * 365))
  return d.toISOString().slice(0, 10)
}

const formatted = (iso: string) =>
  new Date(`${iso}T00:00:00Z`).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric', timeZone: 'UTC' })

/** One row as copied from HUD's income limits table: its label gives the percent of AMI. */
const hudRow = 'Low (60%) Income Limits ($) $60,000 $68,000 $76,000 $84,000 $91,000 $98,000 $104,000 $111,000'

/** A cell of the Enter/Correct Income Limits table: "Percent of AMI", or a household size such as "1 person". */
const cell = (scope: Locator, label: string, nth = 0) =>
  label === 'Percent of AMI'
    ? scope.getByLabel('Percent of AMI', { exact: true }).nth(nth)
    : scope.getByLabel(new RegExp(`of AMI, ${label}$`)).nth(nth)

/** Pastes text into an input the way a browser does, so the page's paste handler runs. */
const paste = (input: Locator, text: string) =>
  input.evaluate((el, t) => {
    const data = new DataTransfer()
    data.setData('text/plain', t)
    el.dispatchEvent(new ClipboardEvent('paste', { clipboardData: data, bubbles: true, cancelable: true }))
  }, text)

test('entering income limits lists the schedule, and correcting it replaces its amounts', async ({ page }) => {
  await page.goto('/income-limits')
  await expect(page.getByRole('heading', { name: 'Income Limits', level: 1 })).toBeVisible()
  await expect(page.getByRole('row', { name: /Jan 1, 2000/ })).toContainText('In effect')

  await page.getByRole('button', { name: 'Enter Income Limits' }).click()
  const enter = panel(page, 'Enter Income Limits')
  const effectiveFrom = futureEffectiveDate()
  await field(enter, 'Effective From').fill(effectiveFrom)
  // The rows start with the newest schedule's levels; replace them with one level.
  while (await enter.getByRole('button', { name: 'Remove' }).count() > 0) {
    await enter.getByRole('button', { name: 'Remove' }).first().click()
  }
  await enter.getByRole('button', { name: 'Add Level' }).click()
  await paste(cell(enter, '1 person'), hudRow)
  await expect(cell(enter, 'Percent of AMI')).toHaveValue('60')
  await expect(cell(enter, '8 people')).toHaveValue('111000')
  await enter.getByRole('button', { name: 'Save Income Limits' }).click()
  await expectToast(page, 'Income limits saved')
  await expect(enter).toHaveCount(0)

  const row = page.getByRole('row', { name: new RegExp(formatted(effectiveFrom)) })
  await expect(row).toContainText('60% of AMI')
  await expect(row).not.toContainText('In effect')

  await row.click()
  const detail = panel(page, `Income Limits from ${formatted(effectiveFrom)}`)
  await expect(detail.getByRole('row', { name: /^60%/ })).toContainText('$60,000')
  await detail.getByRole('button', { name: 'Correct' }).click()

  const correct = panel(page, 'Correct Income Limits')
  await expect(cell(correct, '1 person')).toHaveValue('60000')
  await cell(correct, '1 person').fill('61000')
  await correct.getByRole('button', { name: 'Save Income Limits' }).click()
  await expectToast(page, 'Income limits saved')

  await row.click()
  await expect(panel(page, `Income Limits from ${formatted(effectiveFrom)}`).getByRole('row', { name: /^60%/ })).toContainText('$61,000')
})

test('a schedule with two levels at the same percent is not saved', async ({ page }) => {
  await page.goto('/income-limits')
  await page.getByRole('button', { name: 'Enter Income Limits' }).click()
  const enter = panel(page, 'Enter Income Limits')
  while (await enter.getByRole('button', { name: 'Remove' }).count() > 0) {
    await enter.getByRole('button', { name: 'Remove' }).first().click()
  }
  await enter.getByRole('button', { name: 'Add Level' }).click()
  await enter.getByRole('button', { name: 'Add Level' }).click()
  await cell(enter, 'Percent of AMI', 0).fill('80')
  await cell(enter, 'Percent of AMI', 1).fill('80')
  await enter.getByRole('button', { name: 'Save Income Limits' }).click()
  await expect(enter).toContainText('Two levels have the same percent of AMI.')
})
