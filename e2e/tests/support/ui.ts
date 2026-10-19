import { expect, type Locator, type Page } from '@playwright/test'

/** Unique suffix so tests can run in parallel against a database that already holds data. */
export const uniqueId = () => `${Date.now().toString(36)}${Math.random().toString(36).slice(2, 6)}`

/** The topmost slide-over panel (action panels open on top of detail panels). */
export const topPanel = (page: Page) => page.getByRole('dialog').last()

/** A slide-over panel identified by its header title. */
export const panel = (page: Page, title: string) =>
  page.getByRole('dialog').filter({
    has: page.locator('.slideover-header__title', { hasText: new RegExp(`^${escapeRegExp(title)}$`) }),
  }).last()

/**
 * The input/textarea/select/combobox in a `.form-field` whose `.form-label` matches `label`.
 * The app's labels aren't linked to their inputs, so getByLabel doesn't work.
 */
export const field = (scope: Locator, label: string, nth = 0) =>
  scope
    .locator('.form-field')
    .filter({ has: scope.page().locator('.form-label', { hasText: new RegExp(`^${escapeRegExp(label)}$`) }) })
    .nth(nth)
    .locator('input, textarea, select, [role=combobox]')
    .first()

/** The value cell of a read-only `.detail-field` in a detail panel. */
export const detailValue = (scope: Locator, label: string) =>
  scope
    .locator('.detail-field')
    .filter({ has: scope.page().locator('.detail-field__label', { hasText: new RegExp(`^${escapeRegExp(label)}$`) }) })
    .locator('.detail-field__value')
    .first()

export const expectToast = (page: Page, message: string) =>
  expect(page.locator('[data-sonner-toast]', { hasText: message }).first()).toBeVisible()

/** ISO date (YYYY-MM-DD) offset from today, for date inputs. */
export const isoDate = (offsetDays = 0) => {
  const d = new Date()
  d.setDate(d.getDate() + offsetDays)
  return d.toISOString().slice(0, 10)
}

function escapeRegExp(s: string) {
  return s.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')
}
