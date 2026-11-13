import type { MortgageRequestResponse } from '../../api'
import { MORTGAGE_PURPOSE_LABELS, type LoanAppraisalForm } from '../../hooks/groundLeases'
import { addDays, formatCalendarDate, longDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'

/**
 * Business days (Monday to Friday) after `from` up to and including `to`; public holidays count. Matches the API's
 * count, so the form shows what the request will show.
 */
export function businessDaysBetween(from: CalendarDate, to: CalendarDate): number {
  let days = 0
  for (let day = addDays(from, 1); day <= to; day = addDays(day, 1)) {
    const weekday = new Date(`${day}T00:00:00Z`).getUTCDay()
    if (weekday !== 0 && weekday !== 6) days++
  }
  return days
}

const percent = (n: number) => `${n.toLocaleString('en-US', { maximumFractionDigits: 3 })}%`

/** "Refinance with First Federal". */
export const mortgageRequestTitle = (r: Pick<MortgageRequestResponse, 'purpose' | 'lender'>) =>
  `${MORTGAGE_PURPOSE_LABELS[r.purpose ?? ''] ?? r.purpose} with ${r.lender}`

/** "6.25% fixed, 30 years". */
export const loanTermsText = (r: Pick<MortgageRequestResponse, 'interestRatePercent' | 'rateType' | 'termYears'>) =>
  `${percent(r.interestRatePercent ?? 0)} ${(r.rateType ?? '').toLowerCase()}, ${r.termYears} ${r.termYears === 1 ? 'year' : 'years'}`

/** The debt check's sentence, or null until the appraisal is in. */
export function debtCheckText(r: MortgageRequestResponse): { share: string; verdict: string; overCap: boolean } | null {
  if (r.purchaseOptionPrice == null || r.debtCapAmount == null || r.debtPercent == null) return null
  const overCap = !!r.overCap
  return {
    share: `Total debt after closing is ${percent(r.debtPercent)} of the purchase option price.`,
    verdict: overCap
      ? `Above the cap by ${formatPrice((r.totalDebtAfterClosing ?? 0) - r.debtCapAmount)}.`
      : 'Within the cap.',
    overCap,
  }
}

export interface MortgageRequestLetter {
  date: string
  recipient: string[]
  heading: string
  greeting: string
  paragraphs: string[]
  signOff: string[]
  cc: string
}

/**
 * The trust's permission letter for an approved loan (Art 8.1), or its refusal for a denied one. The approval is
 * the trust's written permission, so it names the terms and the debt check figures it was given on.
 */
export function mortgageRequestLetter(
  r: MortgageRequestResponse,
  homeLabel: string | null,
  today: CalendarDate,
  signOff: string,
): MortgageRequestLetter {
  const name = r.homeownerFullName ?? 'Homeowner'
  const purpose = (MORTGAGE_PURPOSE_LABELS[r.purpose ?? ''] ?? r.purpose ?? '').toLowerCase()
  const principal = formatPrice(r.principal ?? 0)
  const asked = r.receivedOn ? ` dated ${formatCalendarDate(r.receivedOn, longDate)}` : ''
  const approved = r.status !== 'Denied'

  const paragraphs = approved
    ? [
      `Thank you for your request${asked} for a ${purpose} loan from ${r.lender}. The trust gives its written permission `
        + `for this loan under your ground lease, on these terms: a principal of ${principal} at ${loanTermsText(r)}, `
        + `with total mortgage debt on the home of ${formatPrice(r.totalDebtAfterClosing ?? 0)} once it closes.`,
      ...(r.purchaseOptionPrice != null && r.maxMortgageDebtPercent != null && r.debtPercent != null
        ? [
          `Based on the appraisal of ${formatPrice(r.appraisedValue ?? 0)}, the purchase option price under your lease is `
            + `${formatPrice(r.purchaseOptionPrice)}. Your lease limits total mortgage debt to ${r.maxMortgageDebtPercent}% `
            + `of that price, ${formatPrice(r.debtCapAmount ?? 0)}. The total debt after this loan is `
            + `${percent(r.debtPercent)} of the price.`,
        ]
        : []),
      'This permission is for the terms above only. A loan on other terms needs a new request to the trust.',
      'Once the loan closes, please send us a copy of the recorded mortgage.',
    ]
    : [
      `Thank you for your request${asked} for a ${purpose} loan of ${principal} from ${r.lender}. The trust cannot `
        + 'permit this loan under your ground lease.',
      r.decisionReason ?? '',
      'You may send a new request for a loan on other terms. If you would like to talk about what the trust could '
        + 'permit, please contact us.',
    ].filter(Boolean)

  return {
    date: formatCalendarDate(today, longDate),
    recipient: [name, ...(homeLabel ? [homeLabel] : [])],
    heading: approved ? 'Permission for a mortgage loan' : 'Mortgage loan not permitted',
    greeting: `Dear ${name},`,
    paragraphs,
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
    cc: `cc: ${r.lender}`,
  }
}

/** Opens a decided request's letter in a new tab, where the print dialog opens. */
export const openMortgageRequestLetter = (mortgageRequestId: string) =>
  window.open(`/mortgage-requests/${encodeURIComponent(mortgageRequestId)}/letter`, '_blank')

/** Whether the appraisal fields are complete enough to send: empty is fine unless required. */
export function appraisalValid(appraisal: LoanAppraisalForm, indexBased: boolean, required: boolean): boolean {
  if (appraisal.appraisedValue <= 0) return !required
  return !!appraisal.appraisalDate && (!indexBased || appraisal.currentIndexValue > 0)
}
