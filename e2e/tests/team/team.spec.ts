import { expect, test } from '@playwright/test'
import { post } from '../support/api'
import { expectToast, panel, topPanel, uniqueId } from '../support/ui'

// Journeys:
//   docs/journeys/team/invite-staff — a member makes an invite link, sees it pending, cancels one
//   docs/journeys/onboarding/join-a-trust — a colleague opens the link, registers, joins and lands on Today
//   docs/journeys/team/remove-a-member — the member removes the colleague, who loses access at once

test('a colleague joins with an invite link, shows up on the Team page, and is removed', async ({ page, browser }) => {
  const id = uniqueId()
  const email = `colleague-${id}@example.com`

  await page.goto('/team')
  await expect(page.getByRole('heading', { name: 'Team', level: 1 })).toBeVisible()
  await page.getByRole('button', { name: 'Invite Staff' }).click()
  const invite = panel(page, 'Invite Staff')
  await invite.getByLabel('Email').fill(email)
  await invite.getByRole('button', { name: 'Create Invite Link' }).click()
  const linkField = invite.getByLabel('Invite Link')
  await expect(linkField).toHaveValue(/\/invite\/[\w-]+$/)
  const link = await linkField.inputValue()
  await invite.getByRole('button', { name: 'Done' }).click()

  const pendingRow = page.getByRole('table', { name: 'Pending Invites' }).getByRole('row', { name: new RegExp(email) })
  await expect(pendingRow).toBeVisible()

  // The colleague: a fresh browser with no session, registering a new account from the link.
  const colleagueContext = await browser.newContext({ storageState: { cookies: [], origins: [] } })
  const colleague = await colleagueContext.newPage()
  await colleague.goto(link)
  await colleague.getByRole('link', { name: 'Register' }).click()
  await colleague.locator('#username').fill(`colleague-${id}`)
  await colleague.locator('#email').fill(email)
  await colleague.locator('#firstName').fill('Invited')
  await colleague.locator('#lastName').fill(`Colleague ${id}`)
  await colleague.locator('#password').fill('e2e-password')
  await colleague.locator('#password-confirm').fill('e2e-password')
  await colleague.getByRole('button', { name: 'Register' }).click()

  await expect(colleague.getByRole('heading', { name: /^Join / })).toBeVisible()
  await expect(colleague.getByText(`This invite was made for ${email}.`)).toBeVisible()
  await colleague.getByRole('button', { name: 'Join Trust' }).click()
  await expect(colleague.getByRole('heading', { name: 'Today', level: 1 })).toBeVisible()

  // Opening the link again: the colleague is in a trust now.
  await colleague.goto(link)
  await expect(colleague.getByRole('heading', { name: 'You already belong to a trust.' })).toBeVisible()

  const memberRow = page.getByRole('table', { name: 'Members' }).getByRole('row', { name: new RegExp(`Colleague ${id}`) })
  await page.goto('/team')
  await expect(memberRow).toContainText(email)
  await expect(pendingRow).toHaveCount(0)
  await expect(page.getByRole('table', { name: 'Members' }).getByRole('row', { name: /\(you\)/ }).getByRole('button', { name: 'Remove' })).toHaveCount(0)

  await memberRow.getByRole('button', { name: 'Remove' }).click()
  await expect(topPanel(page)).toContainText('will lose access to the trust at once')
  await topPanel(page).getByRole('button', { name: 'Remove Member' }).click()
  await expectToast(page, `Invited Colleague ${id} removed`)
  await expect(memberRow).toHaveCount(0)

  // The colleague's session still names the trust; their next request is refused and they land on onboarding.
  await colleague.goto('/')
  await expect(colleague.getByRole('heading', { name: "You're not part of a trust yet" })).toBeVisible()
  await colleagueContext.close()
})

test('a member cancels a pending invite', async ({ page }) => {
  const email = `cancelled-${uniqueId()}@example.com`
  await post(page, '/api/v1/trust-invitations', { email })

  await page.goto('/team')
  const row = page.getByRole('table', { name: 'Pending Invites' }).getByRole('row', { name: new RegExp(email) })
  await row.getByRole('button', { name: 'Cancel' }).click()
  await expect(topPanel(page)).toContainText(`The link sent to ${email} will stop working.`)
  await topPanel(page).getByRole('button', { name: 'Cancel Invite' }).click()

  await expectToast(page, 'Invite cancelled')
  await expect(row).toHaveCount(0)
})
