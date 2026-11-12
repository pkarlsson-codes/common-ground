import { expect, test, type Locator, type Page } from '@playwright/test'
import { createApplication, createHome, createLandParcel, uploadHomeDocument } from '../support/api'
import { detailValue, expectToast, field, panel, uniqueId } from '../support/ui'

// Journeys: docs/journeys/homes/
//   register-home, view-home-detail, record-home-assessment, upload-home-document,
//   remove-home-document, manage-home-document-versions, initiate-development-project-from-home,
//   set-home-income-limit

/** Narrows the Homes table to the homes on one parcel (each test creates its own parcel). */
async function filterByParcel(page: Page, apn: string) {
  await page.getByRole('combobox').filter({ hasText: 'Filter by parcel…' }).click()
  await page.getByPlaceholder('Filter by parcel…').fill(apn)
  await page.getByRole('listbox').getByRole('option', { name: new RegExp(apn) }).click()
  await expect(page.locator('tbody tr')).toHaveCount(1)
  return page.locator('tbody tr').first()
}

/** Seeds a Single Family home on a fresh parcel and opens its detail panel. */
async function openSeededHome(page: Page) {
  await page.goto('/homes')
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  await (await filterByParcel(page, apn)).click()
  return { detail: panel(page, `${apn} Elm St`), homeId, apn }
}

const assessmentsSection = (detail: Locator) =>
  detail.locator('.detail-section').filter({ has: detail.page().locator('.detail-section__title', { hasText: /^Assessments$/ }) })

/** The dates, values, and appraisers of the listed assessments, top to bottom. */
async function expectAssessments(detail: Locator, rows: [date: string, value: string, appraiser: string][]) {
  const section = assessmentsSection(detail)
  await expect(section.locator('span.detail-field__label')).toHaveText(rows.map(r => r[0]))
  await expect(section.locator('span.detail-field__value.mono')).toHaveText(rows.map(r => r[1]))
  await expect(section.locator('span.detail-field__value:not(.mono)')).toHaveText(rows.map(r => r[2]))
}

const documentsTab = (detail: Locator) => detail.getByRole('button', { name: /^documents$/i })

/** The Version History panel expanded beneath a document row. */
const versionHistory = (detail: Locator) =>
  detail.locator('div')
    .filter({ has: detail.page().getByText('Version History', { exact: true }) })
    .filter({ has: detail.page().getByRole('button', { name: 'DL' }) })
    .last()

/** Clicks a version's DL button and returns the downloaded file's text. */
async function downloadVersion(page: Page, dl: Locator) {
  const request = page.context().waitForEvent('request', r => r.url().includes('/download?versionId='))
  const popup = page.waitForEvent('popup')
  await dl.click()
  const url = (await request).url()
  await (await popup).close()
  return (await page.request.get(url)).text()
}

test('registering a home adds it to the registry with its details and initial assessment', async ({ page }) => {
  await page.goto('/homes')
  const { apn } = await createLandParcel(page)

  await page.getByRole('button', { name: 'Register Home' }).click()
  const register = panel(page, 'Register Home')
  await field(register, 'Land Parcel').click()
  await page.getByPlaceholder('Search parcels…').fill(apn)
  await page.getByRole('listbox').getByRole('option', { name: `${apn} — ${apn} Elm St` }).click()
  await field(register, 'Home Type').selectOption('Duplex')
  await field(register, 'Unit Designation (optional)').fill('Unit B')
  await field(register, 'Bedrooms').fill('4')
  await field(register, 'Bathrooms').fill('2.5')
  await register.getByRole('checkbox', { name: 'Accessible home' }).check()
  await field(register, 'Square Footage').fill('1500')
  await field(register, 'Year Built').fill('1978')
  await field(register, 'Initial Assessed Value ($)').fill('310000')
  await field(register, 'Assessment Date').fill('2024-05-20')
  await field(register, 'Appraiser Name').fill('Reggie Appraiser')
  await register.getByRole('button', { name: 'Register Home' }).click()
  await expectToast(page, 'Home registered successfully')
  await expect(register).toBeHidden()

  const row = await filterByParcel(page, apn)
  await expect(row).toContainText('Duplex')
  await expect(row).toContainText('4 bd / 2.5 ba')
  await expect(row).toContainText('1,500')
  await expect(row).toContainText('$310,000')
  await expect(row).toContainText('Available')

  await expect(row).toContainText(`${apn} Elm St, Unit B`)

  // The filters find it as Vacant and Offered, not as Occupied and Not For Sale (Leased).
  const occupancyFilter = page.getByLabel('Occupancy')
  const saleStageFilter = page.getByLabel('Sale stage')
  await occupancyFilter.selectOption('Occupied')
  await saleStageFilter.selectOption('NotForSale')
  await expect(page.locator('.empty-state')).toBeVisible()
  await occupancyFilter.selectOption('Vacant')
  await saleStageFilter.selectOption('Offered')
  await expect(page.locator('tbody tr')).toHaveCount(1)

  await row.click()
  const detail = panel(page, `${apn} Elm St, Unit B`)
  await expect(detail).toContainText('Duplex · 4 bd · 2.5 ba · 1,500 sq ft')
  await expect(detailValue(detail, 'Status')).toHaveText('Available')
  await expect(detailValue(detail, 'Accessible')).toHaveText('Yes')
  await expect(detailValue(detail, 'Year Built')).toHaveText('1978')
  await expect(detailValue(detail, 'Assessed Value')).toHaveText('$310,000')
  await expect(detailValue(detail, 'Last Assessed')).toHaveText('2024-05-20')
  await expect(detail.locator('.detail-field__label', { hasText: 'Condition Notes' })).toHaveCount(0)
  await expect(detailValue(detail, 'Registered At')).not.toBeEmpty()
  await expect(detail).toContainText('Development Projects (0)')
  await expectAssessments(detail, [['2024-05-20', '$310,000', 'Reggie Appraiser']])

  // The parcel link opens the parcel's own detail panel.
  await detail.getByRole('button', { name: 'Open Land Parcel' }).click()
  await expect(page).toHaveURL(/\/land-parcels$/)
  await expect(panel(page, apn)).toContainText(`${apn} Elm St`)
})

test('setting a home income limit warns when the limits in effect lack that level', async ({ page }) => {
  const { detail } = await openSeededHome(page)
  await expect(detailValue(detail, 'Income Limit')).toHaveText('80% of AMI')

  const nextStep = detail.getByRole('region', { name: 'Next step' })
  await nextStep.getByRole('button', { name: 'More actions' }).click()
  await nextStep.getByRole('menuitem', { name: 'Set Income Limit' }).click()
  const set = panel(page, 'Set Income Limit')
  await expect(field(set, 'Percent of AMI')).toHaveValue('80')
  await expect(set.getByRole('status')).toHaveCount(0)

  await field(set, 'Percent of AMI').fill('60')
  await expect(set.getByRole('status')).toContainText('The income limits in effect have no 60% of AMI level.')
  await set.getByRole('button', { name: 'Save' }).click()
  await expectToast(page, 'Income limit saved')
  await expect(detailValue(detail, 'Income Limit')).toHaveText('60% of AMI')

  await nextStep.getByRole('button', { name: 'More actions' }).click()
  await nextStep.getByRole('menuitem', { name: 'Set Income Limit' }).click()
  await set.getByText('No income limit').click()
  await expect(set.getByRole('status')).toHaveCount(0)
  await set.getByRole('button', { name: 'Save' }).click()
  await expectToast(page, 'Income limit saved')
  await expect(detailValue(detail, 'Income Limit')).toHaveText('No income limit')
})

test('a selection for a home whose income limit level is not in effect is refused', async ({ page }) => {
  // Someone must be waiting, or the start is refused for that reason instead.
  await createApplication(page, `Waiting ${uniqueId()}`)
  await page.goto('/homes')
  const { parcelId, apn } = await createLandParcel(page)
  await createHome(page, parcelId, { incomeLimitPercentOfAmi: 60 })
  await (await filterByParcel(page, apn)).click()
  const nextStep = panel(page, `${apn} Elm St`).getByRole('region', { name: 'Next step' })

  // HomeBuyers learns about the new home from an integration event, so retry until the start is refused for its limit.
  await expect(async () => {
    await nextStep.getByRole('button', { name: 'Start Selection' }).click()
    await expect(nextStep).toContainText('No income limits for 60% of AMI are in effect', { timeout: 2000 })
  }).toPass()
})

test('recording an assessment lists it first and updates the home details', async ({ page }) => {
  const { detail } = await openSeededHome(page)
  await expect(detailValue(detail, 'Accessible')).toHaveText('No')
  await expectAssessments(detail, [['2024-01-01', '$280,000', 'E2E Appraiser']])

  await detail.getByRole('button', { name: 'Add New Assessment' }).click()
  const record = detail.getByRole('button', { name: 'Record Assessment' })
  await expect(record).toBeDisabled()
  await field(detail, 'Assessed Value ($)').fill('295000')
  await field(detail, 'Assessment Date').fill('2025-06-10')
  await field(detail, 'Appraiser Name (optional)').fill('Ada Appraiser')
  await field(detail, 'Condition Notes (optional)').fill('New roof')
  await record.click()
  await expectToast(page, 'Assessment recorded')

  await expect(detail.getByRole('button', { name: 'Add New Assessment' })).toBeVisible()
  await expect(record).toHaveCount(0)
  await expectAssessments(detail, [
    ['2025-06-10', '$295,000', 'Ada Appraiser'],
    ['2024-01-01', '$280,000', 'E2E Appraiser'],
  ])
  await expect(detailValue(detail, 'Assessed Value')).toHaveText('$295,000')
  await expect(detailValue(detail, 'Last Assessed')).toHaveText('2025-06-10')
  await expect(detailValue(detail, 'Condition Notes')).toHaveText('New roof')

  // Without an appraiser name, the assessment is listed with "—".
  await detail.getByRole('button', { name: 'Add New Assessment' }).click()
  await field(detail, 'Assessed Value ($)').fill('300000')
  await field(detail, 'Assessment Date').fill('2025-09-01')
  await detail.getByRole('button', { name: 'Record Assessment' }).click()
  await expectToast(page, 'Assessment recorded')
  await expectAssessments(detail, [
    ['2025-09-01', '$300,000', '—'],
    ['2025-06-10', '$295,000', 'Ada Appraiser'],
    ['2024-01-01', '$280,000', 'E2E Appraiser'],
  ])
})

test('uploading a document attaches it to the home', async ({ page }) => {
  const { detail } = await openSeededHome(page)
  await documentsTab(detail).click()
  await expect(detail).toContainText('No documents attached yet.')

  await detail.getByRole('button', { name: 'Upload Document' }).click()
  const upload = detail.getByRole('button', { name: 'Upload', exact: true })
  await expect(upload).toBeDisabled()

  // The title is filled in from the file name, and can be changed.
  await detail.locator('input[type=file]').setInputFiles({
    name: 'inspection-report.txt', mimeType: 'text/plain', buffer: Buffer.from('All good'),
  })
  await expect(field(detail, 'Title')).toHaveValue('inspection-report')
  await expect(field(detail, 'Sensitivity')).toHaveValue('Internal')
  await expect(field(detail, 'Retention')).toHaveValue('Standard3yr')
  const title = `Inspection ${uniqueId()}`
  await field(detail, 'Title').fill(title)
  await field(detail, 'Sensitivity').selectOption('Confidential')
  await field(detail, 'Retention').selectOption('LongTerm7yr')
  await upload.click()
  await expectToast(page, 'Document uploaded')

  await expect(detail.getByRole('button', { name: 'Upload Document' })).toBeVisible()
  await expect(detail.locator('input[type=file]')).toHaveCount(0)
  await expect(detail.getByText(title, { exact: true })).toBeVisible()
  await expect(detail.getByText('inspection-report.txt', { exact: true })).toBeVisible()
  await expect(detail).not.toContainText('No documents attached yet.')
})

test('removing a document asks for a reason and takes it off the home', async ({ page }) => {
  const { detail, homeId } = await openSeededHome(page)
  const title = `Old Deed ${uniqueId()}`
  await uploadHomeDocument(page, homeId, title, 'deed.txt', 'Deed')
  await documentsTab(detail).click()
  await expect(detail.getByText(title, { exact: true })).toBeVisible()

  // Cancelling leaves the document in place.
  await detail.getByRole('button', { name: 'Remove' }).click()
  await expect(detail).toContainText(`Remove "${title}"? It can be restored later.`)
  await detail.getByRole('button', { name: 'Cancel' }).click()
  await expect(field(detail, 'Reason')).toHaveCount(0)
  await expect(detail.getByText(title, { exact: true })).toBeVisible()

  await detail.getByRole('button', { name: 'Remove' }).click()
  const confirm = detail.getByRole('button', { name: 'Confirm Remove' })
  await expect(confirm).toBeDisabled()
  await field(detail, 'Reason').fill('Superseded by the recorded deed')
  await confirm.click()
  await expectToast(page, 'Document removed')
  await expect(detail.getByText(title, { exact: true })).toHaveCount(0)
  await expect(detail).toContainText('No documents attached yet.')
})

test('uploading a new version makes it current and each version downloads its own file', async ({ page }) => {
  const { detail, homeId } = await openSeededHome(page)
  const title = `Survey ${uniqueId()}`
  await uploadHomeDocument(page, homeId, title, 'survey-v1.txt', 'first survey')
  await documentsTab(detail).click()

  await detail.getByRole('button', { name: 'Versions' }).click()
  const versions = versionHistory(detail)
  await expect(versions.getByText(/^v\d+$/)).toHaveText(['v1'])
  await expect(versions).toContainText('survey-v1.txt')
  await expect(versions).toContainText('current')

  await versions.getByRole('button', { name: 'Upload New Version' }).click()
  const upload = versions.getByRole('button', { name: 'Upload', exact: true })
  await expect(upload).toBeDisabled()
  await versions.locator('input[type=file]').setInputFiles({
    name: 'survey-v2.txt', mimeType: 'text/plain', buffer: Buffer.from('second survey'),
  })
  await versions.getByPlaceholder('Version note (optional)').fill('Corrected boundary')
  await upload.click()
  await expectToast(page, 'New version uploaded')

  // Newest first; the new version is current, and the document row shows its file.
  await expect(versions.getByRole('button', { name: 'Upload New Version' })).toBeVisible()
  await expect(versions.getByText(/^v\d+$/)).toHaveText(['v2', 'v1'])
  await expect(versions.getByText('current', { exact: true })).toHaveCount(1)
  const current = versions.locator('div').filter({ hasText: 'survey-v2.txt' }).filter({ hasText: 'current' })
  await expect(current.first()).toBeVisible()
  await expect(detail.getByText('survey-v2.txt', { exact: true }).first()).toBeVisible()

  const dl = versions.getByRole('button', { name: 'DL' })
  expect(await downloadVersion(page, dl.nth(0))).toBe('second survey')
  expect(await downloadVersion(page, dl.nth(1))).toBe('first survey')
})

test('initiating a development project from a home locks its parcel and home', async ({ page }) => {
  const { detail, apn } = await openSeededHome(page)

  await detail.getByRole('button', { name: '+ Initiate Project' }).click()
  const initiate = panel(page, 'Initiate Development Project')
  await expect(initiate.getByRole('button', { name: 'Rehabilitation' })).toHaveClass(/btn-primary/)
  await expect(field(initiate, 'Land Parcel')).toBeDisabled()
  await expect(field(initiate, 'Land Parcel')).toHaveText(`${apn} — ${apn} Elm St`)
  await expect(field(initiate, 'Homes')).toBeDisabled()
  await expect(field(initiate, 'Homes')).toContainText('Single Family — 3bd / 2ba')

  const name = `Rehab ${uniqueId()}`
  await field(initiate, 'Name').fill(name)
  await expect(initiate.getByRole('button', { name: 'Initiate Project' })).toBeDisabled()
  await field(initiate, 'Description').fill('Replace windows and siding')
  await initiate.getByRole('button', { name: 'Initiate Project' }).click()
  await expectToast(page, 'Development project initiated')
  await expect(initiate).toBeHidden()

  await expect(detail).toContainText('Development Projects (1)')
  await expect(detail).toContainText(name)
})
