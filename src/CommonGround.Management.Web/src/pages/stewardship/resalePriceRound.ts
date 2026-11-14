import type { ResalePriceStatementLeaseResponse } from '../../api'
import type { RoundLeaseFigures } from '../../hooks/groundLeases'
import type { CalendarDate } from '../../lib/calendarDate'
import { cannotPriceReason, emptyStatementFigures, statementFiguresComplete } from '../ground-leases/resalePriceStatementLetter'

export type RoundLease = ResalePriceStatementLeaseResponse

/** A market value typed for one appraisal-based lease; a blank source takes the one from the row above. */
export interface MarketValueEntry {
  value: number | null
  source: string
}

/** What staff enter once for a round: one value per index type, one market value per appraisal-based lease. */
export interface RoundEntries {
  indexValues: Record<string, number | null>
  marketValues: Record<string, MarketValueEntry>
}

/** The index types the round asks for, each with the baselines of its leases, in the order the leases come. */
export function roundIndexTypes(leases: RoundLease[]): { indexType: string; baselines: number[] }[] {
  const byType = new Map<string, number[]>()
  for (const lease of leases) {
    if (lease.formulaKind !== 'IndexBased' || !lease.indexType) continue
    const baselines = byType.get(lease.indexType) ?? []
    if (lease.baselineIndexValue != null && !baselines.includes(lease.baselineIndexValue)) baselines.push(lease.baselineIndexValue)
    byType.set(lease.indexType, baselines)
  }
  return [...byType].map(([indexType, baselines]) => ({ indexType, baselines }))
}

/** The appraisal-based leases that can be priced, which each need a market value row. */
export const marketValueLeases = (leases: RoundLease[]): RoundLease[] =>
  leases.filter(l => l.formulaKind === 'AppraisalBased' && !cannotPriceReason(l))

/** Each market value row's source, with a blank one filled down from the nearest row above that has one. */
export function filledSources(rows: RoundLease[], marketValues: Record<string, MarketValueEntry>): Record<string, string> {
  const result: Record<string, string> = {}
  let above = ''
  for (const lease of rows) {
    const own = marketValues[lease.groundLeaseId!]?.source.trim() ?? ''
    if (own) above = own
    result[lease.groundLeaseId!] = own || above
  }
  return result
}

/** The figures for each lease whose formula has what it needs; leases still missing a figure are left out. */
export function roundFigures(leases: RoundLease[], entries: RoundEntries): RoundLeaseFigures[] {
  const sources = filledSources(marketValueLeases(leases), entries.marketValues)
  return leases.flatMap(lease => {
    const id = lease.groundLeaseId!
    const figures = {
      ...emptyStatementFigures(),
      marketValue: lease.formulaKind === 'AppraisalBased' ? entries.marketValues[id]?.value ?? null : null,
      marketValueSource: lease.formulaKind === 'AppraisalBased' ? sources[id] ?? '' : '',
      indexValue: lease.formulaKind === 'IndexBased' ? entries.indexValues[lease.indexType ?? ''] ?? null : null,
    }
    return statementFiguresComplete(lease, figures) ? [{ groundLeaseId: id, figures }] : []
  })
}

/** What the round's print page needs: the statement date and the figures of each lease to print. */
export interface PrintedRound {
  statementDate: CalendarDate
  leases: RoundLeaseFigures[]
}

const ROUND_KEY_PREFIX = 'resale-price-statement-round:'

/**
 * Keeps the round for the print page, which opens in a new tab, and returns its key. The figures of a few hundred
 * leases are too long for the page's address. Only the latest round is kept. Null when the browser won't store it.
 */
export function savePrintedRound(round: PrintedRound): string | null {
  try {
    for (let i = localStorage.length - 1; i >= 0; i--) {
      const key = localStorage.key(i)
      if (key?.startsWith(ROUND_KEY_PREFIX)) localStorage.removeItem(key)
    }
    const id = crypto.randomUUID()
    localStorage.setItem(ROUND_KEY_PREFIX + id, JSON.stringify(round))
    return id
  } catch {
    return null
  }
}

/** The round saved under the key, or null when it is gone or the browser won't read it. */
export function loadPrintedRound(id: string | null): PrintedRound | null {
  if (!id) return null
  try {
    const text = localStorage.getItem(ROUND_KEY_PREFIX + id)
    return text ? (JSON.parse(text) as PrintedRound) : null
  } catch {
    return null
  }
}

export const printedRoundPath = (id: string) => `/resale-price-statements/print?round=${encodeURIComponent(id)}`
