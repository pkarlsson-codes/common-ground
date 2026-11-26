import { expect, test, type Locator, type Page } from '@playwright/test'
import { createApplication, createPurchaseWorkflow, get, post } from '../support/api'
import { isoDate, panel, uniqueId } from '../support/ui'

// Journeys:
//   docs/journeys/homes/view-home-detail — the home's stepper, next step and connected records
//   docs/journeys/households/view-household-detail — the household's stepper, next step and connected records
//   docs/journeys/purchase-workflows/fail-purchase-workflow — a failed purchase sends both back a stage

const currentStep = (detail: Locator) => detail.locator('[aria-current="step"]')
const nextStep = (detail: Locator) => detail.getByRole('region', { name: 'Next step' })

async function openHousehold(page: Page, email: string, name: string) {
  await page.goto('/households')
  await page.getByRole('searchbox', { name: 'Search households' }).fill(email)
  await page.getByRole('row', { name: new RegExp(name) }).click()
  return panel(page, name)
}

test('a household selected for a home is purchasing it, and the two detail pages link to each other', async ({ page }) => {
  const buyer = `Buyer ${uniqueId()}`
  const { apn } = await createPurchaseWorkflow(page, buyer, { fromApplication: true })
  const email = `${buyer.toLowerCase().replace(/\s+/g, '-')}@example.com`
  const homeLabel = `${apn} Elm St`

  const household = await openHousehold(page, email, buyer)
  await expect(currentStep(household)).toContainText('Purchasing')
  for (const done of ['Intake', 'Ready', 'Approved', 'Waiting', 'Selected']) {
    await expect(household.locator('.lifecycle-step--done', { hasText: done })).toHaveCount(1)
  }
  await expect(nextStep(household)).toContainText(`Buying ${homeLabel}: waiting on eligibility verification.`)
  await expect(nextStep(household).getByRole('button', { name: 'Open Purchase' })).toBeVisible()

  await household.getByRole('button', { name: `Open Home: ${homeLabel}` }).click()
  await expect(page).toHaveURL(/\/homes$/)
  const home = panel(page, homeLabel)
  await expect(currentStep(home)).toContainText('Purchasing')
  await expect(nextStep(home)).toContainText(`The purchase by ${buyer} is waiting on eligibility verification.`)
  await expect(home.getByRole('button', { name: /^Open Selection: Chosen / })).toBeVisible()
  await expect(home.getByRole('button', { name: `Open Purchase: ${buyer}` })).toBeVisible()

  await home.getByRole('button', { name: `Open Household: ${buyer}` }).click()
  await expect(page).toHaveURL(/\/households$/)
  await expect(currentStep(panel(page, buyer))).toContainText('Purchasing')
})

test('a failed purchase returns the household to the waiting list and makes the home available, with no selection open', async ({ page }) => {
  const buyer = `Fallen ${uniqueId()}`
  const { apn, workflowId, householdId, homeId } = await createPurchaseWorkflow(page, buyer, { fromApplication: true })
  const email = `${buyer.toLowerCase().replace(/\s+/g, '-')}@example.com`
  const homeLabel = `${apn} Elm St`
  // Someone else is waiting, and still nobody is chosen until staff choose.
  await createApplication(page, `Next ${uniqueId()}`)

  await post(page, `/api/v1/purchase-workflows/${workflowId}/fail`, { failureReason: 'Financing fell through', failedDate: isoDate() })
  // HomeBuyers and PropertyRegistry hear about it from an integration event, so wait for both.
  await expect.poll(async () =>
    (await get<{ journey: { stage: string } }>(page, `/api/v1/households/${householdId}`)).journey.stage).toBe('Waiting')
  await expect.poll(async () => (await get<{ saleStage: string }>(page, `/api/v1/homes/${homeId}`)).saleStage).toBe('Offered')
  await expect.poll(async () =>
    (await get<{ isOffered: boolean; decisions: unknown[] }>(page, `/api/v1/homes/${homeId}/selection`)))
    .toMatchObject({ isOffered: true, decisions: [expect.anything()] })

  const household = await openHousehold(page, email, buyer)
  await expect(currentStep(household)).toContainText('Waiting')
  await expect(household.locator('.lifecycle-step--done', { hasText: 'Selected' })).toHaveCount(0)
  await expect(household.getByRole('button', { name: /^Open Purchase:/ })).toContainText('Failed')

  await household.getByRole('button', { name: `Open Home: ${homeLabel}` }).click()
  const home = panel(page, homeLabel)
  await expect(currentStep(home)).toContainText('Available')
  await expect(nextStep(home)).toContainText('1 household was chosen for this home before.')
  await expect(nextStep(home).getByRole('button', { name: 'Choose a Household' })).toBeVisible()
  await expect(home.getByRole('button', { name: /^Open Selection: Chosen / })).toHaveCount(1)
})
