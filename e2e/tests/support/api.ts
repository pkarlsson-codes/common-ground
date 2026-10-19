import { expect, type APIResponse, type Page } from '@playwright/test'
import { isoDate, uniqueId } from './ui'

// Seeds test data through the gateway (same session as the browser), for steps that aren't
// the journey under test. Mutations need the double-submit CSRF header and an allowed Origin.

const ORIGIN = 'http://localhost:3000'

async function headers(page: Page) {
  const csrf = (await page.context().cookies(ORIGIN)).find(c => c.name === 'cg_csrf')?.value
  if (!csrf) throw new Error('No cg_csrf cookie; is the auth setup project running?')
  return { Origin: ORIGIN, 'X-CSRF-Token': csrf }
}

async function ok<T>(response: APIResponse, what: string): Promise<T> {
  expect(response.ok(), `${what}: ${response.status()} ${await response.text()}`).toBeTruthy()
  const text = await response.text()
  return (text ? JSON.parse(text) : undefined) as T
}

export async function post<T = void>(page: Page, path: string, body: unknown): Promise<T> {
  const response = await page.request.post(path, { data: body, headers: await headers(page) })
  return ok<T>(response, `POST ${path}`)
}

export async function get<T>(page: Page, path: string): Promise<T> {
  return ok<T>(await page.request.get(path), `GET ${path}`)
}

export async function createLandParcel(page: Page) {
  const apn = `APN-${uniqueId()}`
  const { parcelId } = await post<{ parcelId: string }>(page, '/api/v1/land-parcels', {
    legalParcelNumber: apn,
    address: `${apn} Elm St`,
    areaAcres: 0.25,
    acquisitionDate: '2024-01-01',
    acquisitionMethod: 'Purchase',
    covenantTerms: null,
  })
  return { parcelId, apn }
}

export async function createHome(page: Page, landParcelId: string) {
  const { homeId } = await post<{ homeId: string }>(page, '/api/v1/homes', {
    landParcelId,
    homeType: 'SingleFamily',
    bedrooms: 3,
    bathrooms: 2,
    squareFootage: 1200,
    yearBuilt: 1990,
    appraiserName: 'E2E Appraiser',
    initialAssessedDate: '2024-01-01',
    initialAssessedValue: 280000,
  })
  return homeId
}

export async function createGroundLeaseTemplate(page: Page) {
  const templateName = `E2E Template ${uniqueId()}`
  const { groundLeaseTemplateId } = await post<{ groundLeaseTemplateId: string }>(page, '/api/v1/ground-lease-templates', {
    templateName,
    termYears: 99,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
    indexType: null,
    baselineIndexValue: null,
    annualPercentage: null,
    improvementsCap: null,
  })
  return { groundLeaseTemplateId, templateName }
}

/**
 * Registers a one-member household, records its readiness, and submits its application; approves it unless told
 * otherwise. `ready: false` skips CLT orientation, leaving readiness incomplete.
 */
export async function createApplication(page: Page, name: string, { approve = true, ready = true } = {}) {
  const email = `${name.toLowerCase().replace(/\s+/g, '-')}@example.com`
  const { householdId } = await post<{ householdId: string }>(page, '/api/v1/households', {
    members: [{ name, email, phone: '+12025550143', isPrimary: true }],
    requiresAccessibility: false,
    annualIncome: 65000,
  })
  await post(page, `/api/v1/households/${householdId}/housing-counseling`, {
    certificationNumber: 'CERT-E2E', completedDate: isoDate(-7), counselorOrganization: 'E2E Counseling Org',
  })
  if (ready) {
    await post(page, `/api/v1/households/${householdId}/clt-orientation`, { completedDate: isoDate(-3) })
  }
  await post(page, `/api/v1/households/${householdId}/mortgage-prequalification`, {
    lenderName: 'E2E Lender', approvedAmount: 210000, expiryDate: isoDate(90),
  })
  const { applicationId } = await post<{ applicationId: string }>(page, '/api/v1/homeowner-applications', { householdId })
  if (approve) {
    await post(page, `/api/v1/homeowner-applications/${applicationId}/approve`, { approvedDate: isoDate(), notes: null })
  }
  return { applicationId, householdId, name, email }
}

/**
 * Starts a purchase workflow on a fresh parcel, home, and lease template. By default it's initiated directly; with
 * `fromApplication` it's started the way the app does it, by selecting an approved applicant for the home.
 */
export async function createPurchaseWorkflow(page: Page, buyerName: string, { fromApplication = false } = {}) {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseTemplateId, templateName } = await createGroundLeaseTemplate(page)
  const purchasePrice = 200000

  let applicationId: string | undefined
  if (fromApplication) {
    applicationId = (await createApplication(page, buyerName)).applicationId
    await post(page, `/api/v1/homeowner-applications/${applicationId}/select`, {
      homeId, groundLeaseTemplateId, purchasePrice, selectedDate: isoDate(),
    })
    // The workflow is started by an integration event, so poll for it.
    await expect.poll(async () => {
      const workflows = await get<{ items: unknown[] }>(
        page, `/api/v1/purchase-workflows?groundLeaseTemplateId=${groundLeaseTemplateId}`)
      return workflows.items.length
    }).toBe(1)
  } else {
    await post(page, '/api/v1/purchase-workflows', {
      groundLeaseTemplateId,
      landParcelId: parcelId,
      homeId,
      buyerFullName: buyerName,
      buyerEmail: `${buyerName.toLowerCase().replace(/\s+/g, '-')}@example.com`,
      buyerPhone: '+12025550143',
      purchasePrice,
    })
  }

  return { parcelId, homeId, groundLeaseTemplateId, templateName, applicationId, buyerName }
}
