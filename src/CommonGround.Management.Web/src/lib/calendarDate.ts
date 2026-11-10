/**
 * A calendar date with no time of day and no time zone, as YYYY-MM-DD: a lease start, a due date,
 * an expiry. The API sends and receives these as plain strings; the brand stops a raw string, or a
 * Date's UTC date, from being passed where a checked one is expected.
 *
 * Instants (timestamps) stay Date. "Today" is the trust's today, not the browser's or UTC's.
 *
 * This module is the only place a string becomes a CalendarDate.
 */
declare const brand: unique symbol
export type CalendarDate = string & { readonly [brand]: 'CalendarDate' }

const pattern = /^(\d{4})-(\d{2})-(\d{2})$/

const toUtcDate = (date: CalendarDate) => {
  const [, y, m, d] = pattern.exec(date)!
  return new Date(Date.UTC(Number(y), Number(m) - 1, Number(d)))
}

const fromUtcDate = (date: Date) => date.toISOString().slice(0, 10) as CalendarDate

export const isCalendarDate = (value: string): value is CalendarDate => {
  const match = pattern.exec(value)
  if (!match) return false
  const [, y, m, d] = match.map(Number)
  const date = new Date(Date.UTC(y, m - 1, d))
  return date.getUTCFullYear() === y && date.getUTCMonth() === m - 1 && date.getUTCDate() === d
}

/** Checks a YYYY-MM-DD string, such as an API value or a date input's value. Throws when it isn't a real date. */
export const parseCalendarDate = (value: string): CalendarDate => {
  if (!isCalendarDate(value)) throw new Error(`Not a calendar date (YYYY-MM-DD): "${value}"`)
  return value
}

/** A date input's value, or null when the input is empty. */
export const calendarDateOrNull = (value: string): CalendarDate | null => (value ? parseCalendarDate(value) : null)

export const addDays = (date: CalendarDate, days: number): CalendarDate => {
  const d = toUtcDate(date)
  d.setUTCDate(d.getUTCDate() + days)
  return fromUtcDate(d)
}

/** Whole days from one date to another; positive when `to` is later. */
export const daysBetween = (from: CalendarDate, to: CalendarDate) =>
  Math.round((toUtcDate(to).getTime() - toUtcDate(from).getTime()) / 86_400_000)

export const yearOf = (date: CalendarDate) => Number(date.slice(0, 4))

/** Today in the given IANA time zone. */
export const todayIn = (timeZone: string, now = new Date()): CalendarDate =>
  // en-CA formats as YYYY-MM-DD.
  new Intl.DateTimeFormat('en-CA', { timeZone, year: 'numeric', month: '2-digit', day: '2-digit' }).format(now) as CalendarDate

const browserTimeZone = () => Intl.DateTimeFormat().resolvedOptions().timeZone
let trustTimeZone: string | undefined

/** Set once the signed-in user's trust has loaded; trustToday() follows it from then on. */
export const setTrustTimeZone = (timeZone: string | undefined) => {
  trustTimeZone = timeZone
}

/** Today in the trust's time zone (the browser's until the trust has loaded), offset by whole days. */
export const trustToday = (offsetDays = 0): CalendarDate => addDays(todayIn(trustTimeZone ?? browserTimeZone()), offsetDays)

export const numericDate: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'numeric', day: 'numeric' }

/** "Oct 28, 2026" by default; "10/28/2026" with numericDate. */
export const formatCalendarDate = (
  date: CalendarDate,
  options: Intl.DateTimeFormatOptions = { year: 'numeric', month: 'short', day: 'numeric' },
) => toUtcDate(date).toLocaleDateString('en-US', { ...options, timeZone: 'UTC' })

const activeTimeZone = () => trustTimeZone ?? browserTimeZone()

/** An instant's date in the trust's time zone, e.g. "Oct 28, 2026". */
export const formatInstantDate = (instant: Date) =>
  instant.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric', timeZone: activeTimeZone() })

/** An instant's date and time in the trust's time zone, e.g. "Oct 28, 2026, 10:31 AM". */
export const formatInstant = (instant: Date) =>
  instant.toLocaleString('en-US', {
    year: 'numeric', month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit', timeZone: activeTimeZone(),
  })
