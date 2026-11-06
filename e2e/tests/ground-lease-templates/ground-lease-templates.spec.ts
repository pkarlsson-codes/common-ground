import { expect, test, type Locator, type Page } from '@playwright/test'
import { createGroundLeaseTemplate } from '../support/api'
import { detailValue, expectToast, field, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/ground-lease-templates/
//   create-ground-lease-template, view-ground-lease-template-detail, archive-ground-lease-template,
//   restore-ground-lease-template

/** Narrows the templates table to one template by searching for its (unique) name. */
async function searchTemplates(page: Page, name: string) {
  await page.getByRole('searchbox', { name: 'Search ground lease templates' }).fill(name)
  await expect(page.locator('tbody tr')).toHaveCount(1)
  return page.getByRole('row', { name: new RegExp(name, 'i') })
}

async function openNewTemplate(page: Page) {
  await page.getByRole('button', { name: 'New Template' }).click()
  return panel(page, 'New Template')
}

/** Fills the fields every formula shares and returns the template's name. */
async function fillCommonFields(form: Locator) {
  const name = `Template ${uniqueId()}`
  await field(form, 'Template Name').fill(name)
  await field(form, 'Covenant Terms').fill('Owner-occupied, permanently affordable')
  return name
}

async function createAndOpen(page: Page, form: Locator, name: string) {
  await form.getByRole('button', { name: 'Create Template' }).click()
  await expectToast(page, 'Ground lease template created')
  await expect(form).toBeHidden()
  const row = await searchTemplates(page, name)
  await expect(row).toContainText('Active')
  await row.click()
  return panel(page, name)
}

const detailLabels = (detail: Locator) => detail.locator('.detail-field__label')

test('creating an appraisal-based template with an improvements cap shows its terms in the detail panel', async ({ page }) => {
  await page.goto('/ground-lease-templates')
  const form = await openNewTemplate(page)

  await expect(field(form, 'Term (years)')).toHaveValue('99')
  await expect(field(form, 'Option Window (days)')).toHaveValue('60')
  await expect(field(form, 'Resale Formula')).toHaveValue('AppraisalBased')
  await expect(field(form, 'Appreciation Share')).toHaveValue('0.25')
  await expect(field(form, 'Improvements Cap ($)')).toHaveCount(0)

  const name = await fillCommonFields(form)
  await field(form, 'Term (years)').fill('75')
  await field(form, 'Option Window (days)').fill('90')
  await field(form, 'Appreciation Share').fill('0.3')
  await form.getByRole('checkbox', { name: 'Set improvements cap' }).check()
  await expect(field(form, 'Improvements Cap ($)')).toHaveValue('25000')
  await field(form, 'Improvements Cap ($)').fill('30000')

  const detail = await createAndOpen(page, form, name)
  await expect(detail.locator('.slideover-header')).toContainText('75 yr · Active')
  await expect(detailValue(detail, 'Status')).toHaveText('Active')
  await expect(detailValue(detail, 'Formula')).toHaveText('Appraisal Based')
  await expect(detailValue(detail, 'Term')).toHaveText('75 years')
  await expect(detailValue(detail, 'Option Window')).toHaveText('90 days')
  await expect(detailValue(detail, 'Appreciation Share')).toHaveText('30.0%')
  await expect(detailValue(detail, 'Improvements Cap')).toHaveText('$30,000')
  await expect(detailValue(detail, 'Covenant Terms')).toHaveText('Owner-occupied, permanently affordable')
  await expect(detailValue(detail, 'Created')).not.toBeEmpty()
  // Fields of the other formulas aren't shown.
  await expect(detailLabels(detail).filter({ hasText: /^(Index Type|Baseline Index|Annual %)$/ })).toHaveCount(0)

  await page.keyboard.press('Escape')
  await expect(detail).toBeHidden()
})

test('creating an index-based template records its index type and baseline', async ({ page }) => {
  await page.goto('/ground-lease-templates')
  const form = await openNewTemplate(page)
  const name = await fillCommonFields(form)

  await field(form, 'Resale Formula').selectOption('IndexBased')
  await expect(field(form, 'Appreciation Share')).toHaveCount(0)
  await expect(field(form, 'Index Type')).toHaveValue('CPI')
  await expect(field(form, 'Baseline Index Value')).toHaveValue('100')
  await field(form, 'Index Type').selectOption('AMI')
  await field(form, 'Baseline Index Value').fill('120.5')

  const detail = await createAndOpen(page, form, name)
  await expect(detailValue(detail, 'Formula')).toHaveText('Index Based')
  await expect(detailValue(detail, 'Term')).toHaveText('99 years')
  await expect(detailValue(detail, 'Index Type')).toHaveText('AMI')
  await expect(detailValue(detail, 'Baseline Index')).toHaveText('120.5')
  await expect(detailLabels(detail).filter({ hasText: /^(Appreciation Share|Annual %|Improvements Cap)$/ })).toHaveCount(0)
})

test('creating a fixed-percentage template records its annual percentage', async ({ page }) => {
  await page.goto('/ground-lease-templates')
  const form = await openNewTemplate(page)
  const name = await fillCommonFields(form)

  await field(form, 'Resale Formula').selectOption('FixedPercentage')
  await expect(field(form, 'Annual Percentage')).toHaveValue('0.01')
  await field(form, 'Annual Percentage').fill('0.03')

  const detail = await createAndOpen(page, form, name)
  await expect(detailValue(detail, 'Formula')).toHaveText('Fixed Percentage')
  await expect(detailValue(detail, 'Annual %')).toHaveText('3.0%')
  await expect(detailLabels(detail).filter({ hasText: /^(Appreciation Share|Index Type|Baseline Index)$/ })).toHaveCount(0)
})

test('a cancelled new template form starts fresh when reopened', async ({ page }) => {
  await page.goto('/ground-lease-templates')
  let form = await openNewTemplate(page)
  await field(form, 'Template Name').fill('Abandoned draft')
  await field(form, 'Resale Formula').selectOption('FixedPercentage')
  await form.getByRole('button', { name: 'Cancel' }).click()
  await expect(form).toBeHidden()

  form = await openNewTemplate(page)
  await expect(field(form, 'Template Name')).toHaveValue('')
  await expect(field(form, 'Resale Formula')).toHaveValue('AppraisalBased')
})

test('archiving and restoring a template moves it between the Active and Inactive filters', async ({ page }) => {
  await page.goto('/ground-lease-templates')
  const { templateName } = await createGroundLeaseTemplate(page)
  const statusFilter = page.locator('.filter-bar select')

  // Search ignores case.
  const row = await searchTemplates(page, templateName.toLowerCase())
  await row.click()
  let detail = panel(page, templateName)
  await expect(detail.getByRole('button', { name: 'Restore' })).toHaveCount(0)
  await detail.getByRole('button', { name: 'Archive' }).click()
  await expectToast(page, 'Template archived')
  await expect(detail).toBeHidden()
  await expect(row).toContainText('Inactive')

  await statusFilter.selectOption('Active')
  await expect(page.locator('.empty-state')).toBeVisible()
  await statusFilter.selectOption('Inactive')
  await expect(page.locator('tbody tr')).toHaveCount(1)

  await row.click()
  detail = panel(page, templateName)
  await expect(detail.locator('.slideover-header')).toContainText('99 yr · Inactive')
  await expect(detail.getByRole('button', { name: 'Archive' })).toHaveCount(0)
  await detail.getByRole('button', { name: 'Restore' }).click()
  await expectToast(page, 'Template restored')
  await expect(detail).toBeHidden()

  await expect(page.locator('.empty-state')).toBeVisible()
  await statusFilter.selectOption('Active')
  await expect(row).toContainText('Active')
})
