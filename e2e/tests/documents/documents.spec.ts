import { expect, test, type Locator, type Page } from '@playwright/test'
import {
  addDocumentVersion,
  createHome,
  createLandParcel,
  setDocumentExpiry,
  tagDocument,
  uploadHomeDocument,
} from '../support/api'
import { expectToast, field, isoDate, panel, uniqueId } from '../support/ui'

// Journeys (docs/journeys/documents/):
//   browse-document-library  — search, tag + file type filters, clear, download, open a row
//   view-expiring-documents  — banner, expiring list (soonest first, Expired/Expires), download, back to all
//   edit-document-details    — edit title/sensitivity/expiry, add/remove tags, versions, open the attached home

const rows = (page: Page) => page.getByTestId('document-row')
const row = (page: Page, title: string) =>
  rows(page).filter({ has: page.getByText(title, { exact: true }) })

/** Clicks a Download button and returns the file it serves (the popup goes through the gateway's download redirect). */
async function download(page: Page, button: Locator) {
  const request = page.context().waitForEvent('request', r => r.url().includes('/download'))
  const popup = page.waitForEvent('popup')
  await button.click()
  const url = (await request).url()
  await (await popup).close()
  return (await page.request.get(url)).text()
}

async function seedHomeDocuments(page: Page, ...docs: { title: string; fileName: string; contents: string }[]) {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const ids: string[] = []
  for (const d of docs) {
    ids.push((await uploadHomeDocument(page, homeId, d.title, d.fileName, d.contents)).documentId)
  }
  return { homeId, ids }
}

const fmtExpiry = (iso: string) =>
  new Date(iso).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric', timeZone: 'UTC' })

test('the library searches by title, filters by tag and file type, and downloads the current version', async ({ page }) => {
  const id = uniqueId()
  const deed = `Deed ${id}`
  const survey = `Survey ${id}`
  await page.goto('/documents')
  const { ids: [deedId] } = await seedHomeDocuments(page,
    { title: deed, fileName: 'deed.txt', contents: 'the deed' },
    { title: survey, fileName: 'survey.txt', contents: 'the survey' })
  await tagDocument(page, deedId, `Recorded-${id}`)
  await page.reload()

  await expect(page.getByRole('heading', { name: 'Documents', level: 1 })).toBeVisible()

  // Search is a case-insensitive title match.
  const search = page.getByRole('searchbox', { name: 'Search documents' })
  await search.fill(id.toUpperCase())
  await expect(rows(page)).toHaveCount(2)
  await search.fill(`deed ${id}`)
  await expect(rows(page)).toHaveCount(1)
  await expect(row(page, deed)).toContainText('deed.txt')
  await expect(row(page, deed)).toContainText(`#recorded-${id}`)

  // Clicking a tag chip filters by it; the survey has no tag.
  await page.getByRole('button', { name: 'Clear filters' }).click()
  await expect(search).toHaveValue('')
  await search.fill(id)
  await expect(rows(page)).toHaveCount(2)
  await row(page, deed).getByRole('button', { name: `Filter by tag recorded-${id}` }).click()
  await expect(page.getByRole('textbox', { name: 'Filter by tag' })).toHaveValue(`recorded-${id}`)
  await expect(rows(page)).toHaveCount(1)
  await expect(row(page, deed)).toBeVisible()

  // A typed tag is applied lowercased.
  await page.getByRole('textbox', { name: 'Filter by tag' }).fill(`RECORDED-${id}`)
  await page.getByRole('button', { name: 'Apply' }).click()
  await expect(rows(page)).toHaveCount(1)

  // File type: both are text files.
  await page.getByRole('button', { name: 'Clear filters' }).click()
  await search.fill(id)
  await page.getByRole('combobox', { name: 'File type' }).selectOption('PDF')
  await expect(page.getByText('No documents match the current filters.')).toBeVisible()
  await page.getByRole('combobox', { name: 'File type' }).selectOption('Text')
  await expect(rows(page)).toHaveCount(2)

  await page.getByRole('button', { name: 'Clear filters' }).click()
  await expect(page.getByRole('combobox', { name: 'File type' })).toHaveValue('')
  await expect(page.getByRole('button', { name: 'Clear filters' })).toBeHidden()

  await search.fill(deed)
  expect(await download(page, row(page, deed).getByRole('button', { name: 'Download' }))).toBe('the deed')

  // Clicking the row opens its detail panel.
  await row(page, deed).click()
  await expect(panel(page, deed)).toBeVisible()
})

test('editing a document updates its details and tags, lists its versions, and links to its home', async ({ page }) => {
  const title = `Lease Packet ${uniqueId()}`
  const renamed = `${title} (signed)`
  await page.goto('/documents')
  const { ids: [documentId] } = await seedHomeDocuments(page,
    { title, fileName: 'packet-v1.txt', contents: 'draft packet' })
  await addDocumentVersion(page, documentId, 'packet-v2.txt', 'signed packet')
  await page.reload()

  await page.getByRole('searchbox', { name: 'Search documents' }).fill(title)
  await expect(row(page, title)).toContainText('v2')
  await row(page, title).click()
  let detail = panel(page, title)
  await expect(detail.locator('.slideover-header__subtitle')).toHaveText('packet-v2.txt')

  // Details: nothing changed yet, so Save is disabled.
  const save = detail.getByRole('button', { name: 'Save Details' })
  await expect(save).toBeDisabled()
  await expect(field(detail, 'Sensitivity')).toHaveValue('Internal')
  await field(detail, 'Title').fill(renamed)
  await field(detail, 'Sensitivity').selectOption('Confidential')
  const expiresOn = isoDate(200)
  await field(detail, 'Expires On').fill(expiresOn)
  await save.click()
  await expectToast(page, 'Document updated')

  detail = panel(page, renamed)
  await expect(detail).toContainText(`Expires ${fmtExpiry(expiresOn)}`)
  await expect(detail.getByRole('button', { name: 'Save Details' })).toBeDisabled()
  await expect(row(page, title)).toHaveCount(0)
  await page.getByRole('searchbox', { name: 'Search documents' }).fill(renamed)
  await expect(row(page, renamed)).toContainText('Confidential')
  await expect(row(page, renamed)).toContainText(`expires ${fmtExpiry(expiresOn)}`)

  // Clearing the expiry removes it.
  await field(detail, 'Expires On').fill('')
  await detail.getByRole('button', { name: 'Save Details' }).click()
  await expectToast(page, 'Document updated')
  await expect(row(page, renamed)).not.toContainText('expires')

  // Tags are trimmed and lowercased; duplicates are ignored; × removes one.
  await expect(detail).toContainText('No tags.')
  const addTag = detail.getByRole('button', { name: 'Add Tag' })
  await expect(addTag).toBeDisabled()
  await detail.getByRole('textbox', { name: 'New tag' }).fill('  Lease File ')
  await addTag.click()
  await expect(detail.getByRole('textbox', { name: 'New tag' })).toHaveValue('')
  await detail.getByRole('textbox', { name: 'New tag' }).fill('lease file')
  await addTag.click()
  await expect(detail.getByRole('button', { name: /^Remove tag/ })).toHaveCount(1)
  await expect(row(page, renamed)).toContainText('#lease file')
  await detail.getByRole('button', { name: 'Remove tag lease file' }).click()
  await expect(detail).toContainText('No tags.')
  await expect(row(page, renamed)).not.toContainText('#lease file')

  // Versions, newest first; each downloads its own file.
  await expect(detail.getByRole('button', { name: /^Download version/ })).toHaveCount(2)
  expect(await download(page, detail.getByRole('button', { name: 'Download version 2' }))).toBe('signed packet')
  expect(await download(page, detail.getByRole('button', { name: 'Download version 1' }))).toBe('draft packet')

  // Attached To links to the home.
  await detail.getByRole('button', { name: 'Open Home' }).click()
  await expect(page).toHaveURL(/\/homes$/)
  await expect(page.getByRole('dialog').filter({ hasText: 'Single Family · 3 bd' })).toBeVisible()
})

test('expired and soon-expiring documents are listed soonest first from the banner', async ({ page }) => {
  const id = uniqueId()
  const expired = `Expired Insurance ${id}`
  const soon = `Expiring Insurance ${id}`
  await page.goto('/documents')
  const { ids: [expiredId, soonId] } = await seedHomeDocuments(page,
    { title: expired, fileName: 'insurance-2025.txt', contents: 'old policy' },
    { title: soon, fileName: 'insurance-2026.txt', contents: 'current policy' })
  const expiredOn = isoDate(-1)
  const soonOn = isoDate(20)

  // Expiries are cleared afterwards so later runs' documents stay on the first page of the expiring list.
  try {
    await setDocumentExpiry(page, soonId, soonOn)
    await setDocumentExpiry(page, expiredId, expiredOn)
    await page.reload()

    const banner = page.getByText(/document(s)? expired or expiring within 90 days/)
    await expect(banner).toBeVisible()
    await page.getByRole('button', { name: 'Show expiring' }).click()
    await expect(page.getByRole('button', { name: 'Show all' })).toBeVisible()
    await expect(page.getByRole('searchbox', { name: 'Search documents' })).toBeHidden()

    await expect(row(page, expired)).toContainText(`Expired ${fmtExpiry(expiredOn)}`)
    await expect(row(page, expired)).toContainText('insurance-2025.txt')
    await expect(row(page, soon)).toContainText(`Expires ${fmtExpiry(soonOn)}`)
    const texts = await rows(page).allTextContents()
    expect(texts.findIndex(t => t.includes(expired))).toBeLessThan(texts.findIndex(t => t.includes(soon)))

    expect(await download(page, row(page, soon).getByRole('button', { name: 'Download' }))).toBe('current policy')

    await row(page, soon).click()
    await expect(panel(page, soon)).toContainText(`Expires ${fmtExpiry(soonOn)}`)
    await page.keyboard.press('Escape')

    await page.getByRole('button', { name: 'Show all' }).click()
    await expect(page.getByRole('searchbox', { name: 'Search documents' })).toBeVisible()
  } finally {
    await setDocumentExpiry(page, expiredId, null)
    await setDocumentExpiry(page, soonId, null)
  }
})
