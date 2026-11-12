import { expect, test } from '@playwright/test'
import { expectToast, field, panel } from '../support/ui'

// Journeys (docs/journeys/income-limits/):
//   enter-income-limits — list, enter and correct a schedule

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

const limits = [60000, 68000, 76000, 84000, 91000, 98000, 104000, 111000]

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
  await field(enter, 'Percent of AMI').fill('60')
  for (let size = 1; size <= 8; size++) {
    await field(enter, size === 1 ? '1 person' : `${size} people`).fill(String(limits[size - 1]))
  }
  await enter.getByRole('button', { name: 'Save Income Limits' }).click()
  await expectToast(page, 'Income limits saved')
  await expect(enter).toHaveCount(0)

  const row = page.getByRole('row', { name: new RegExp(formatted(effectiveFrom)) })
  await expect(row).toContainText('60% of AMI')
  await expect(row).not.toContainText('In effect')

  await row.click()
  const detail = panel(page, `Income Limits from ${formatted(effectiveFrom)}`)
  await expect(detail.getByRole('row', { name: /^1 / })).toContainText('$60,000')
  await detail.getByRole('button', { name: 'Correct' }).click()

  const correct = panel(page, 'Correct Income Limits')
  await expect(field(correct, '1 person')).toHaveValue('60000')
  await field(correct, '1 person').fill('61000')
  await correct.getByRole('button', { name: 'Save Income Limits' }).click()
  await expectToast(page, 'Income limits saved')

  await row.click()
  await expect(panel(page, `Income Limits from ${formatted(effectiveFrom)}`).getByRole('row', { name: /^1 / })).toContainText('$61,000')
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
  await field(enter, 'Percent of AMI', 0).fill('80')
  await field(enter, 'Percent of AMI', 1).fill('80')
  await enter.getByRole('button', { name: 'Save Income Limits' }).click()
  await expect(enter).toContainText('Two levels have the same percent of AMI.')
})
