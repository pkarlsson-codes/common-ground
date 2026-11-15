import type { PayoffStatementResponse } from '../../api'
import { addDays, formatCalendarDate, isCalendarDate, longDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'

/** How many days ahead of today a payoff statement can be good through. */
export const PAYOFF_MAX_DAYS_AHEAD = 60

export interface PayoffStatementLetter {
  date: string
  recipient: string[]
  heading: string
  greeting: string
  paragraphs: string[]
  working: string[]
  signOff: string[]
}

/** The resale's homeowner and home, as the letter names them. */
export interface PayoffResale {
  homeownerFullName?: string | null
  homeLabel?: string | null
}

/** The letter's address: the day it is good through. */
export const payoffStatementLetterPath = (resaleId: string, goodThrough: CalendarDate) =>
  `/resales/${encodeURIComponent(resaleId)}/payoff-statement?goodThrough=${goodThrough}`

/** The Good Through day in the letter's address, or null when it is missing or not a date. */
export const parsePayoffStatementLetterParams = (params: URLSearchParams): CalendarDate | null => {
  const value = params.get('goodThrough') ?? ''
  return isCalendarDate(value) ? value : null
}

/** Whether Good Through is a day the server accepts: today up to 60 days ahead. */
export const goodThroughValid = (goodThrough: CalendarDate | null, today: CalendarDate): goodThrough is CalendarDate =>
  !!goodThrough && goodThrough >= today && goodThrough <= addDays(today, PAYOFF_MAX_DAYS_AHEAD)

const longDay = (date: CalendarDate) => formatCalendarDate(date, longDate)

/** The working, one line per figure, as the panel and the letter show it. */
export function payoffWorking(statement: PayoffStatementResponse): string[] {
  const lines = [`Lease fees owed: ${formatPrice(statement.feesOwed ?? 0)}`, `Interest owed: ${formatPrice(statement.interestOwed ?? 0)}`]
  if ((statement.credit ?? 0) > 0) lines.push(`Credit: ${formatPrice(statement.credit ?? 0)}`)
  if (statement.oldestUnpaidMonth) lines.push(`Oldest unpaid month: ${longDay(statement.oldestUnpaidMonth)}`)
  return lines
}

/** The trust's payoff request to the closing agent: what to pay from the seller's proceeds (Art 5.8). */
export function payoffStatementLetter(
  resale: PayoffResale,
  statement: PayoffStatementResponse,
  today: CalendarDate,
  signOff: string,
): PayoffStatementLetter {
  const homeowner = resale.homeownerFullName ?? 'the homeowner'
  const home = resale.homeLabel ?? 'the home'
  const goodThrough = longDay(statement.goodThrough!)
  const total = statement.totalOwed ?? 0

  return {
    date: longDay(today),
    recipient: ['Closing Agent', ...(resale.homeLabel ? [resale.homeLabel] : [])],
    heading: `Payoff statement for ${home}`,
    greeting: 'Dear Closing Agent,',
    paragraphs: [
      total > 0
        ? `${homeowner} is selling ${home}. Under Art 5.8 of the ground lease, unpaid lease fees are paid to the trust from the seller's proceeds. `
          + `As of ${goodThrough}, the amount owed is ${formatPrice(total)}.`
        : `${homeowner} is selling ${home}. As of ${goodThrough}, nothing is owed to the trust under the ground lease.`,
      ...(total > 0 ? [`Please pay ${formatPrice(total)} to the trust at closing. If closing is after ${goodThrough}, ask us for a new statement.`] : []),
    ],
    working: payoffWorking(statement),
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
  }
}
