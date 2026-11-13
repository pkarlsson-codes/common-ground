import type { PurchaseIncomeLimitLevelDto } from '../api'

/** The household sizes a published income limit table prints a limit for. */
export const PRINTED_HOUSEHOLD_SIZES = 8

export interface IncomeBand {
  overPercentOfAmi?: number | null
  upToPercentOfAmi?: number | null
}

/**
 * The text staff see for an income band: "Up to 30% of AMI", "Over 30% to 50% of AMI" or "Over 120% of AMI".
 * Null when no band was recorded.
 */
export function incomeBandLabel(band: IncomeBand): string | null {
  const { overPercentOfAmi: over, upToPercentOfAmi: upTo } = band
  if (over == null && upTo == null) return null
  if (over == null) return `Up to ${upTo}% of AMI`
  if (upTo == null) return `Over ${over}% of AMI`
  return `Over ${over}% to ${upTo}% of AMI`
}

/**
 * The band an income falls in, the way the API records it: the lowest level whose limit for the household size
 * is at or above the income. Null above 8 people, where the API works the limits out.
 */
export function incomeBandFor(
  levels: PurchaseIncomeLimitLevelDto[],
  annualIncome: number,
  householdSize: number,
): IncomeBand | null {
  if (householdSize < 1 || householdSize > PRINTED_HOUSEHOLD_SIZES) return null

  let over: number | null = null
  for (const level of [...levels].sort((a, b) => (a.percentOfAmi ?? 0) - (b.percentOfAmi ?? 0))) {
    const limit = level.limits?.[householdSize - 1]
    if (limit == null || level.percentOfAmi == null) continue
    if (annualIncome <= limit) return { overPercentOfAmi: over, upToPercentOfAmi: level.percentOfAmi }
    over = level.percentOfAmi
  }
  return over == null ? null : { overPercentOfAmi: over, upToPercentOfAmi: null }
}
