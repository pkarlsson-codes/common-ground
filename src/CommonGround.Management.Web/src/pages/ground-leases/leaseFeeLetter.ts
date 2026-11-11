import { formatCalendarDate, parseCalendarDate, type CalendarDate } from '../../lib/calendarDate'
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

/**
 * The text of a late notice. The wording is the same for every trust; the sign-off is the trust's own. It names no
 * lease section, since trusts number their leases differently.
 */
export function leaseFeeLetter(lease: LetterLease, level: string, today: CalendarDate, signOff: string): LeaseFeeLetter {
  const name = lease.homeownerFullName ?? 'Homeowner'
  const balance = formatPrice(lease.balance ?? 0)
  const fee = formatPrice(lease.monthlyFee ?? 0)
  const months = lease.oldestUnpaidDueDate ? unpaidMonths(lease.oldestUnpaidDueDate, today) : null
  const date = formatCalendarDate(today, { year: 'numeric', month: 'long', day: 'numeric' })
  const second = level === 'Second'

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
  paragraphs.push(
    'Please pay the amount due as soon as you can. If you have already sent your payment, thank you, and please disregard this letter.',
  )
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
    heading: second ? 'Second notice: lease fee past due' : 'Lease fee past due',
    greeting: `Dear ${name},`,
    paragraphs,
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
  }
}

/** Opens the print page for one lease's notice, or every notice due, in a new tab. */
export const openNoticeLetters = (groundLeaseId?: string) =>
  window.open(`/lease-fee-notices/print${groundLeaseId ? `?lease=${encodeURIComponent(groundLeaseId)}` : ''}`, '_blank')
