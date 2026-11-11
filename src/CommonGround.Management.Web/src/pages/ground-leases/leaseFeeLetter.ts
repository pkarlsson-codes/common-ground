import { addDays, formatCalendarDate, parseCalendarDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { formatDueMonth } from './leaseFeeFormat'

/** What a late notice letter needs from a lease. */
export interface LetterLease {
  homeownerFullName?: string | null
  homeLabel?: string | null
  monthlyFee?: number
  balance?: number
  oldestUnpaidDueDate?: CalendarDate | null
  lateInterestRate?: number
  interestWaiverDays?: number
  interestDue?: number
  interestWaivedIfPaidBy?: CalendarDate | null
}

/** The cure period the trust gives in a notice of default. */
export interface LetterCurePeriod {
  cureDays: number
  extensionDays: number
}

export interface LeaseFeeLetter {
  date: string
  recipient: string[]
  heading: string
  greeting: string
  paragraphs: string[]
  signOff: string[]
}

const monthsBetween = (from: CalendarDate, to: CalendarDate) =>
  (Number(to.slice(0, 4)) - Number(from.slice(0, 4))) * 12 + Number(to.slice(5, 7)) - Number(from.slice(5, 7))

/** "September 2026" for one month; "September 2026 to November 2026" for several. */
export function unpaidMonths(oldestUnpaid: CalendarDate, today: CalendarDate): string {
  const lastDue = parseCalendarDate(`${today.slice(0, 8)}01`)
  return monthsBetween(oldestUnpaid, lastDue) <= 0
    ? formatDueMonth(oldestUnpaid)
    : `${formatDueMonth(oldestUnpaid)} to ${formatDueMonth(lastDue)}`
}

const longDate = (date: CalendarDate) => formatCalendarDate(date, { year: 'numeric', month: 'long', day: 'numeric' })

/**
 * The text of a late notice or notice of default. The wording is the same for every trust; the sign-off and cure
 * period are the trust's own. It names no lease section, since trusts number their leases differently. A notice of
 * default's deadlines run from the letter's date, so it should be sent, and recorded, that day.
 */
export function leaseFeeLetter(
  lease: LetterLease,
  level: string,
  today: CalendarDate,
  signOff: string,
  cure: LetterCurePeriod = { cureDays: 30, extensionDays: 30 },
): LeaseFeeLetter {
  const name = lease.homeownerFullName ?? 'Homeowner'
  const balance = formatPrice(lease.balance ?? 0)
  const fee = formatPrice(lease.monthlyFee ?? 0)
  const months = lease.oldestUnpaidDueDate ? unpaidMonths(lease.oldestUnpaidDueDate, today) : null
  const date = formatCalendarDate(today, { year: 'numeric', month: 'long', day: 'numeric' })
  const second = level === 'Second'
  const notOfDefault = level !== 'Default'

  const rate = lease.lateInterestRate ?? 0
  const interestDue = lease.interestDue ?? 0
  const rateText = `${rate.toLocaleString('en-US', { maximumFractionDigits: 3 })}% a year`

  const paragraphs = [
    `Our records show that your lease fee has not been paid in full. As of ${date}, you owe ${balance}`
      + (months ? `, for the monthly lease fee of ${fee} due for ${months}` : '')
      + (interestDue > 0
        ? `. This includes ${formatPrice(interestDue)} of interest at ${rateText} on fees not paid within ${lease.interestWaiverDays ?? 0} days of their due date.`
        : '.'),
  ]
  if (rate > 0 && lease.interestWaivedIfPaidBy && lease.oldestUnpaidDueDate)
    paragraphs.push(
      `Under your ground lease, interest at ${rateText} is counted on a late fee from the day it was due. If you pay the `
        + `${formatDueMonth(lease.oldestUnpaidDueDate)} fee by ${formatCalendarDate(lease.interestWaivedIfPaidBy, { year: 'numeric', month: 'long', day: 'numeric' })}, `
        + 'no interest will be charged on it.',
    )
  if (notOfDefault)
    paragraphs.push(
      'Please pay the amount due as soon as you can. If you have already sent your payment, thank you, and please disregard this letter.',
    )
  else {
    const cureBy = addDays(today, cure.cureDays)
    paragraphs.push(
      `This is a formal notice of default under your ground lease. To cure the default, you must pay the ${balance} owed `
        + `within ${cure.cureDays} days of the date of this notice, by ${longDate(cureBy)}.`,
    )
    if (cure.extensionDays > 0)
      paragraphs.push(
        `If you pay at least two-thirds of this amount (${formatPrice(Math.round((lease.balance ?? 0) * 200 / 3) / 100)}) by `
          + `${longDate(cureBy)}, you will have ${cure.extensionDays} more days, until `
          + `${longDate(addDays(cureBy, cure.extensionDays))}, to pay the rest.`,
      )
    paragraphs.push(
      'If the default is not cured in time, the trust may end your ground lease or buy your home under the terms of the lease. We would much rather avoid that.',
    )
  }
  if (second)
    paragraphs.push(
      'This is our second notice. Under your ground lease, continued failure to pay the lease fee can lead to a formal notice of default, and a default that is not cured in time puts your lease at risk.',
    )
  paragraphs.push(
    'If you are having trouble paying, please contact us. We would like to talk through your situation and find a way forward with you.',
  )

  return {
    date,
    recipient: [name, ...(lease.homeLabel ? [lease.homeLabel] : [])],
    heading: !notOfDefault ? 'Notice of default: lease fee unpaid' : second ? 'Second notice: lease fee past due' : 'Lease fee past due',
    greeting: `Dear ${name},`,
    paragraphs,
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
  }
}

/** Opens the print page for one lease's notice, or every notice due, in a new tab. */
export const openNoticeLetters = (groundLeaseId?: string) =>
  window.open(`/lease-fee-notices/print${groundLeaseId ? `?lease=${encodeURIComponent(groundLeaseId)}` : ''}`, '_blank')
