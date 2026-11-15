import type { ImprovementRequestResponse } from '../../api'
import { formatCalendarDate, longDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'

export interface ImprovementRequestLetter {
  date: string
  recipient: string[]
  heading: string
  greeting: string
  paragraphs: string[]
  signOff: string[]
}

/** Statuses whose request has a letter: the decision was made and, for an approval, not withdrawn. */
export const improvementLetterApplies = (status: string | undefined) =>
  status === 'Approved' || status === 'Denied' || status === 'Completed'

/**
 * The trust's written reply to a request to alter or add to the home (Art 7.3): its approval with the credit it
 * pre-approved, or its refusal with the reason. A completed request still prints the approval it was given.
 */
export function improvementRequestLetter(
  r: ImprovementRequestResponse,
  homeLabel: string | null,
  today: CalendarDate,
  signOff: string,
): ImprovementRequestLetter {
  const name = r.homeownerFullName ?? 'Homeowner'
  const asked = r.receivedOn ? ` dated ${formatCalendarDate(r.receivedOn, longDate)}` : ''
  const work = r.description ?? 'the work'
  const approved = r.status !== 'Denied'
  const credit = r.preApprovedCredit ?? 0

  const paragraphs = approved
    ? [
      `Thank you for your request${asked} to carry out this work on your home: ${work}. The trust approves it under `
        + 'your ground lease.',
      credit > 0
        ? `The trust pre-approves a credit of ${formatPrice(credit)} for this work toward the resale price of your home, `
          + 'up to the improvements cap in your lease.'
        : 'The trust approves the work without a credit toward the resale price of your home.',
      ...(credit > 0
        ? [
          'The credit counts only after the trust has received your receipts for the work and has confirmed that the '
            + 'work is finished. Please send the receipts when the work is done, and tell us when we can see the result.',
        ]
        : []),
    ]
    : [
      `Thank you for your request${asked} to carry out this work on your home: ${work}. The trust cannot approve it `
        + 'under your ground lease.',
      r.denialReason ?? '',
      'You may send a new request for different work. If you would like to talk about what the trust could approve, '
        + 'please contact us.',
    ].filter(Boolean)

  return {
    date: formatCalendarDate(today, longDate),
    recipient: [name, ...(homeLabel ? [homeLabel] : [])],
    heading: approved ? 'Approval of improvements to your home' : 'Improvements to your home not approved',
    greeting: `Dear ${name},`,
    paragraphs,
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
  }
}

/** Opens a decided request's letter in a new tab, where the print dialog opens. */
export const openImprovementRequestLetter = (improvementRequestId: string) =>
  window.open(`/improvement-requests/${encodeURIComponent(improvementRequestId)}/letter`, '_blank')
