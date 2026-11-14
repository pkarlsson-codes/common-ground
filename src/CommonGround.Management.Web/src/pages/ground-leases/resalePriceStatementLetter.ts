import type { GroundLeaseResponse, ResalePriceStatementPriceResponse } from '../../api'
import type { ResalePriceStatementFigures } from '../../hooks/groundLeases'
import { formatCalendarDate, longDate, parseCalendarDate, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { FORMULA_KIND_LABELS } from '../../hooks/types'

/** What a statement needs from the lease: the homeowner and the resale formula. */
export type StatementLease = Pick<
  GroundLeaseResponse,
  | 'homeownerFullName'
  | 'basePrice'
  | 'originalAppraisedValue'
  | 'formulaKind'
  | 'appreciationShare'
  | 'indexType'
  | 'baselineIndexValue'
  | 'annualPercentage'
  | 'improvementsCap'
>

/** A worked-out price: a preview, or a statement already sent. */
export type StatementPrice = Pick<
  ResalePriceStatementPriceResponse,
  'statementDate' | 'yearsOwned' | 'marketValue' | 'marketValueSource' | 'indexValue' | 'improvementsCredit' | 'price'
>

export const emptyStatementFigures = (): ResalePriceStatementFigures => ({
  marketValue: null,
  marketValueSource: '',
  indexValue: null,
  improvementsCredit: null,
})

/** Why the lease's price can't be worked out, or null when it can. */
export const cannotPriceReason = (lease: StatementLease): string | null =>
  lease.formulaKind === 'AppraisalBased' && lease.originalAppraisedValue == null
    ? "This lease has no original appraised value, so its resale price can't be worked out."
    : null

/** Whether the figures the lease's formula needs are in. */
export function statementFiguresComplete(lease: StatementLease, figures: ResalePriceStatementFigures): boolean {
  if (cannotPriceReason(lease)) return false
  if (lease.formulaKind === 'AppraisalBased') return (figures.marketValue ?? 0) > 0 && figures.marketValueSource.trim() !== ''
  if (lease.formulaKind === 'IndexBased') return (figures.indexValue ?? 0) > 0
  return true
}

const percent = (fraction: number) => `${(fraction * 100).toLocaleString('en-US', { maximumFractionDigits: 3 })}%`
/** "Appraisal Based, 25% of the rise in market value". */
export function formulaText(lease: StatementLease): string {
  const kind = FORMULA_KIND_LABELS[lease.formulaKind ?? ''] ?? lease.formulaKind ?? ''
  if (lease.formulaKind === 'AppraisalBased') return `${kind}, ${percent(lease.appreciationShare ?? 0)} of the rise in market value`
  if (lease.formulaKind === 'IndexBased') return `${kind}, ${lease.indexType ?? 'index'}`
  return `${kind}, ${percent(lease.annualPercentage ?? 0)} a year`
}

const indexNumber = (n: number) => n.toLocaleString('en-US', { minimumFractionDigits: 1, maximumFractionDigits: 4 })
const longDay = (date: CalendarDate) => formatCalendarDate(date, longDate)

/** The working, one line per figure, as the letter prints it. */
export function statementWorking(lease: StatementLease, price: StatementPrice): string[] {
  const base = `Purchase price ${formatPrice(lease.basePrice ?? 0)}`
  const credit = price.improvementsCredit ?? null
  const formulaPrice = formatPrice((price.price ?? 0) - (credit ?? 0))
  const lines: string[] = []

  if (lease.formulaKind === 'AppraisalBased') {
    const market = price.marketValue ?? 0
    const original = lease.originalAppraisedValue ?? 0
    const share = percent(lease.appreciationShare ?? 0)
    const marketText = `market value ${formatPrice(market)}${price.marketValueSource ? ` (${price.marketValueSource})` : ''}`
    lines.push(market >= original
      ? `${base} plus ${share} of the rise in market value: ${marketText} less ${formatPrice(original)} at purchase = ${formulaPrice}`
      : `${base} less ${share} of the fall in market value: ${marketText} against ${formatPrice(original)} at purchase = ${formulaPrice}`)
  } else if (lease.formulaKind === 'IndexBased') {
    const index = lease.indexType ?? 'Index'
    lines.push(
      `${base} × ${index} on ${longDay(price.statementDate!)} ${indexNumber(price.indexValue ?? 0)} `
        + `÷ ${index} at purchase ${indexNumber(lease.baselineIndexValue ?? 0)} = ${formulaPrice}`,
    )
  } else {
    const years = price.yearsOwned ?? 0
    lines.push(`${base} plus ${percent(lease.annualPercentage ?? 0)} a year for ${years} full ${years === 1 ? 'year' : 'years'} = ${formulaPrice}`)
  }

  if (credit != null) lines.push(`Approved improvements credit: ${formatPrice(credit)}`)
  return lines
}

/** The figures used, in one short line for the lease's list of statements. */
export function statementFiguresText(lease: StatementLease, price: StatementPrice): string {
  const parts: string[] = []
  if (price.marketValue != null) {
    parts.push(`Market value ${formatPrice(price.marketValue)}${price.marketValueSource ? `, ${price.marketValueSource}` : ''}`)
  }
  if (price.indexValue != null) parts.push(`${lease.indexType ?? 'Index'} ${indexNumber(price.indexValue)}`)
  if (lease.formulaKind === 'FixedPercentage') parts.push(`${price.yearsOwned} full ${price.yearsOwned === 1 ? 'year' : 'years'}`)
  if (price.improvementsCredit != null) parts.push(`Improvements credit ${formatPrice(price.improvementsCredit)}`)
  return parts.join(' · ')
}

export interface ResalePriceStatementLetter {
  date: string
  recipient: string[]
  heading: string
  greeting: string
  priceSentence: string
  working: string[]
  closing: string
  signOff: string[]
}

/**
 * The yearly letter that reminds a homeowner what their home may sell for under the lease's formula (Ch 23,
 * Disclosure). It says the trust's option price at a sale is the lower of this price and the appraised value then,
 * and that it is not an offer.
 */
export function resalePriceStatementLetter(
  lease: StatementLease,
  homeLabel: string | null,
  price: StatementPrice,
  signOff: string,
): ResalePriceStatementLetter {
  const name = lease.homeownerFullName ?? 'Homeowner'
  const day = longDay(price.statementDate!)
  return {
    date: day,
    recipient: [name, ...(homeLabel ? [homeLabel] : [])],
    heading: "Your home's resale price",
    greeting: `Dear ${name},`,
    priceSentence: `Under your ground lease, the most your home can sell for on ${day} is ${formatPrice(price.price ?? 0)}.`,
    working: statementWorking(lease, price),
    closing: 'If you decide to sell, the trust may buy your home at this price or at its appraised value then, whichever '
      + 'is lower, worked out when you give notice. The price will change with the formula until then. This letter is '
      + 'for your information and is not an offer.',
    signOff: signOff.split('\n').map(l => l.trim()).filter(Boolean),
  }
}

/** The letter page's address: the lease, the statement date and the figures, so the page can work out the price. */
export function resalePriceStatementLetterPath(
  groundLeaseId: string,
  statementDate: CalendarDate,
  figures: ResalePriceStatementFigures,
): string {
  const params = new URLSearchParams({ date: statementDate })
  if (figures.marketValue != null) params.set('marketValue', String(figures.marketValue))
  if (figures.marketValueSource.trim()) params.set('source', figures.marketValueSource.trim())
  if (figures.indexValue != null) params.set('indexValue', String(figures.indexValue))
  if (figures.improvementsCredit != null) params.set('credit', String(figures.improvementsCredit))
  return `/ground-leases/${encodeURIComponent(groundLeaseId)}/resale-price-statement?${params}`
}

/** The statement date and figures back from the letter page's address; null date when it is missing or bad. */
export function parseResalePriceStatementLetterParams(params: URLSearchParams): {
  statementDate: CalendarDate | null
  figures: ResalePriceStatementFigures
} {
  const number = (key: string) => {
    const value = params.get(key)
    if (value == null || value === '') return null
    const n = Number(value)
    return Number.isFinite(n) ? n : null
  }
  let statementDate: CalendarDate | null = null
  try {
    statementDate = parseCalendarDate(params.get('date') ?? '')
  } catch {
    statementDate = null
  }
  return {
    statementDate,
    figures: {
      marketValue: number('marketValue'),
      marketValueSource: params.get('source') ?? '',
      indexValue: number('indexValue'),
      improvementsCredit: number('credit'),
    },
  }
}
