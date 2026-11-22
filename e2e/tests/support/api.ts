import { expect, type APIResponse, type Page } from '@playwright/test'
import { isoDate, uniqueId } from './ui'

// Seeds test data through the gateway (same session as the browser), for steps that aren't
// the journey under test. Mutations need the double-submit CSRF header and an allowed Origin.

const ORIGIN = process.env.E2E_BASE_URL ?? 'http://localhost:3000'

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

export async function patch<T = void>(page: Page, path: string, body: unknown): Promise<T> {
  const response = await page.request.patch(path, { data: body, headers: await headers(page) })
  return ok<T>(response, `PATCH ${path}`)
}

export async function put<T = void>(page: Page, path: string, body: unknown): Promise<T> {
  const response = await page.request.put(path, { data: body, headers: await headers(page) })
  return ok<T>(response, `PUT ${path}`)
}

export async function get<T>(page: Page, path: string): Promise<T> {
  return ok<T>(await page.request.get(path), `GET ${path}`)
}

/** The baseline schedule's date. Tests must not enter schedules between it and today, or they replace it. */
export const BASELINE_INCOME_LIMITS_DATE = '2000-01-01'

/** The baseline schedule's 80% of AMI limits for 1 to 8 people. A one-person household over 70,000 is over it. */
export const BASELINE_80_PERCENT_LIMITS = [70000, 80000, 90000, 99950, 107950, 115950, 123950, 131950]

const scale = (limits: number[], factor: number) => limits.map(l => Math.round((l * factor) / 50) * 50)

/**
 * Enters the income limits every test's homes are judged by, effective 2000-01-01 with 50%, 80% and 120% levels,
 * unless the trust already has them. Without a schedule in effect, a selection for a home with a limit can't start.
 */
export async function ensureBaselineIncomeLimits(page: Page) {
  const schedules = await get<{ effectiveFrom: string }[]>(page, '/api/v1/income-limit-schedules')
  if (schedules.some(s => s.effectiveFrom === BASELINE_INCOME_LIMITS_DATE)) return
  await post(page, '/api/v1/income-limit-schedules', {
    effectiveFrom: BASELINE_INCOME_LIMITS_DATE,
    levels: [
      { percentOfAmi: 50, limits: scale(BASELINE_80_PERCENT_LIMITS, 0.625) },
      { percentOfAmi: 80, limits: BASELINE_80_PERCENT_LIMITS },
      { percentOfAmi: 120, limits: scale(BASELINE_80_PERCENT_LIMITS, 1.5) },
    ],
  })
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

export async function createHome(page: Page, landParcelId: string, { incomeLimitPercentOfAmi = 80 as number | null } = {}) {
  const { homeId } = await post<{ homeId: string }>(page, '/api/v1/homes', {
    landParcelId,
    homeType: 'SingleFamily',
    bedrooms: 3,
    bathrooms: 2,
    isAccessible: false,
    incomeLimitPercentOfAmi,
    squareFootage: 1200,
    yearBuilt: 1990,
    appraiserName: 'E2E Appraiser',
    initialAssessedDate: '2024-01-01',
    initialAssessedValue: 280000,
  })
  return homeId
}

/**
 * Creates an active, appraisal-based ground lease directly (no purchase), started a year ago, with a 90% debt cap,
 * a $200,000 base price and a $250,000 original appraisal. Returns its ID.
 */
export async function createGroundLease(page: Page, homeownerFullName: string) {
  const { parcelId } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseId } = await post<{ groundLeaseId: string }>(page, '/api/v1/ground-leases', {
    landParcelId: parcelId,
    homeId,
    homeownerFullName,
    homeownerEmail: `${homeownerFullName.toLowerCase().replace(/\s+/g, '.')}@example.com`,
    homeownerPhone: null,
    leaseStartDate: isoDate(-365),
    feesTrackedFrom: isoDate(-365),
    openingBalance: 0,
    termYears: 99,
    optionWindowDays: 60,
    maxMortgageDebtPercent: 90,
    landUseFee: 0,
    repairReserveFee: 0,
    basePrice: 200000,
    originalAppraisedValue: 250000,
    covenantTerms: 'Standard covenant terms',
    formulaKind: 'AppraisalBased',
    appreciationShare: 0.25,
  })
  return groundLeaseId
}

export async function createGroundLeaseTemplate(page: Page) {
  const templateName = `E2E Template ${uniqueId()}`
  const { groundLeaseTemplateId } = await post<{ groundLeaseTemplateId: string }>(page, '/api/v1/ground-lease-templates', {
    templateName,
    termYears: 99,
    optionWindowDays: 60,
    maxMortgageDebtPercent: 90,
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
 * otherwise. `ready: false` skips CLT orientation, leaving readiness incomplete. The default income is under the
 * baseline 80% limit for one person.
 */
export async function createApplication(page: Page, name: string, { approve = true, ready = true, annualIncome = 65000 } = {}) {
  const email = `${name.toLowerCase().replace(/\s+/g, '-')}@example.com`
  const { householdId } = await post<{ householdId: string }>(page, '/api/v1/households', {
    members: [{ name, email, phone: '+12025550143', isPrimary: true }],
    requiresAccessibility: false,
    annualIncome,
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
 * `fromApplication` it's started the way the app does it: a home selection picks the approved applicant by priority.
 */
export async function createPurchaseWorkflow(page: Page, buyerName: string, { fromApplication = false } = {}) {
  const { parcelId, apn } = await createLandParcel(page)
  const homeId = await createHome(page, parcelId)
  const { groundLeaseTemplateId, templateName } = await createGroundLeaseTemplate(page)
  const purchasePrice = 200000

  let applicationId: string | undefined
  let householdId: string | undefined
  let workflowId: string
  if (fromApplication) {
    ;({ applicationId, householdId } = await createApplication(page, buyerName))
    // HomeBuyers learns about the new home from an integration event, so retry until it has.
    let selectionId = ''
    await expect(async () => {
      ;({ selectionId } = await post<{ selectionId: string }>(page, `/api/v1/homes/${homeId}/home-selections`, {}))
    }).toPass()
    await post(page, `/api/v1/home-selections/${selectionId}/select-by-priority`, {
      applicationId,
      priorities: ['WaitingListTenure'],
      reason: 'E2E selection',
      groundLeaseTemplateId,
      purchasePrice,
      selectedDate: isoDate(),
    })
    // The workflow is started by an integration event, so poll for it.
    await expect.poll(async () => {
      const workflows = await get<{ items: unknown[] }>(
        page, `/api/v1/purchase-workflows?groundLeaseTemplateId=${groundLeaseTemplateId}`)
      return workflows.items.length
    }).toBe(1)
    const workflows = await get<{ items: { id: string }[] }>(
      page, `/api/v1/purchase-workflows?groundLeaseTemplateId=${groundLeaseTemplateId}`)
    workflowId = workflows.items[0].id
  } else {
    ;({ purchaseWorkflowId: workflowId } = await post<{ purchaseWorkflowId: string }>(page, '/api/v1/purchase-workflows', {
      groundLeaseTemplateId,
      landParcelId: parcelId,
      homeId,
      buyerFullName: buyerName,
      buyerEmail: `${buyerName.toLowerCase().replace(/\s+/g, '-')}@example.com`,
      buyerPhone: '+12025550143',
      purchasePrice,
    }))
  }

  return { workflowId, parcelId, apn, homeId, groundLeaseTemplateId, templateName, applicationId, householdId, buyerName }
}

/**
 * An eligibility verification body whose income is under every baseline limit (the 50% one-person limit is 43,750).
 */
export async function eligibilityVerified(page: Page, verifiedDate: string) {
  const { documentId } = await uploadDocument(page, 'Income document')
  return { verifiedDate, verifiedAnnualIncome: 40000, verifiedHouseholdSize: 1, incomeDocumentIds: [documentId], notes: null }
}

/** A disclosure acknowledgment body with an uploaded signed file. */
export async function disclosureAcknowledged(page: Page, acknowledgedDate: string) {
  const { documentId } = await uploadDocument(page, 'Signed disclosure')
  return { acknowledgedDate, documentId, notes: null }
}

/**
 * Takes a purchase through every track to closing, which creates an active ground lease for the buyer (from the
 * workflow's lease template, so it can be resold). With `fromApplication`, the purchase starts from a home selection,
 * so eligibility verification records the buyer's income band.
 */
export async function createLeaseFromPurchase(page: Page, buyerName: string, { fromApplication = false } = {}) {
  const workflow = await createPurchaseWorkflow(page, buyerName, { fromApplication })
  const path = `/api/v1/purchase-workflows/${workflow.workflowId}`
  await post(page, `${path}/eligibility`, await eligibilityVerified(page, isoDate(-10)))
  await post(page, `${path}/appraisal`, { appraisedValue: 250000, appraisalDate: isoDate(-10), appraiserName: 'E2E Appraiser', notes: null })
  await post(page, `${path}/financing`, { lenderName: 'E2E Lender', committedAmount: 180000, preapprovalLetterDate: isoDate(-9) })
  await post(page, `${path}/inspection`, { inspectionDate: isoDate(-8), inspectorName: 'E2E Inspector', notes: null })
  await post(page, `${path}/title`, { clearedDate: isoDate(-7), titleCompanyName: null })
  await post(page, `${path}/lease-execution`, { signedDate: isoDate(-6) })
  await post(page, `${path}/disclosure`, await disclosureAcknowledged(page, isoDate(-6)))
  await post(page, `${path}/closing/schedule`, { scheduledClosingDate: isoDate(-1) })
  await post(page, `${path}/closing/record`, { actualClosingDate: isoDate(-1), notes: null })

  // The lease is created by an event handler, so poll for it.
  let leaseId = ''
  await expect.poll(async () => {
    const leases = await get<{ items: { id: string }[] }>(page, `/api/v1/ground-leases?homeId=${workflow.homeId}`)
    leaseId = leases.items[0]?.id ?? ''
    return leaseId
  }).not.toBe('')

  return { ...workflow, leaseId, buyerEmail: `${buyerName.toLowerCase().replace(/\s+/g, '-')}@example.com` }
}

/** Uploads a text file as a document attached to a home. */
export async function uploadHomeDocument(page: Page, homeId: string, title: string, fileName: string, contents: string) {
  const response = await page.request.post('/api/v1/documents', {
    headers: await headers(page),
    multipart: {
      metadata: JSON.stringify({
        title, sensitivity: 'Internal', retention: 'Standard3yr',
        initialAttachment: { entityType: 'Building', entityId: homeId },
      }),
      file: { name: fileName, mimeType: 'text/plain', buffer: Buffer.from(contents) },
    },
  })
  return ok<{ documentId: string }>(response, 'POST /api/v1/documents')
}

/** Uploads a text file as a document attached to nothing. */
export async function uploadDocument(page: Page, title: string) {
  const response = await page.request.post('/api/v1/documents', {
    headers: await headers(page),
    multipart: {
      metadata: JSON.stringify({ title, sensitivity: 'Internal', retention: 'Standard3yr' }),
      file: { name: 'document.txt', mimeType: 'text/plain', buffer: Buffer.from(title) },
    },
  })
  return ok<{ documentId: string }>(response, 'POST /api/v1/documents')
}

/** Adds a text file as a new version of a document. */
export async function addDocumentVersion(page: Page, documentId: string, fileName: string, contents: string) {
  const response = await page.request.post(`/api/v1/documents/${documentId}/versions`, {
    headers: await headers(page),
    multipart: {
      metadata: JSON.stringify({ note: null }),
      file: { name: fileName, mimeType: 'text/plain', buffer: Buffer.from(contents) },
    },
  })
  return ok<{ versionId: string }>(response, `POST /api/v1/documents/${documentId}/versions`)
}

/** Sets (or, with null, clears) a document's expiry date. */
export const setDocumentExpiry = (page: Page, documentId: string, expiresOn: string | null) =>
  patch(page, `/api/v1/documents/${documentId}`, expiresOn ? { expiresOn } : { clearExpiresOn: true })

export const tagDocument = (page: Page, documentId: string, tag: string) =>
  post(page, `/api/v1/documents/${documentId}/tags`, { tag })

/**
 * Closes a purchase so the buyer has an active ground lease, then waits for Stewardship to open the lease's
 * stewardship case (it does so from an integration event).
 */
export async function createStewardshipCase(page: Page, buyerName: string) {
  const lease = await createLeaseFromPurchase(page, buyerName)
  let caseId = ''
  await expect.poll(async () => {
    const queue = await get<{ stewardshipCase: { id: string; groundLeaseId: string } }[]>(
      page, '/api/v1/stewardship/queue/cases')
    caseId = queue.find(i => i.stewardshipCase.groundLeaseId === lease.leaseId)?.stewardshipCase.id ?? ''
    return caseId
  }).not.toBe('')
  return { ...lease, caseId }
}
