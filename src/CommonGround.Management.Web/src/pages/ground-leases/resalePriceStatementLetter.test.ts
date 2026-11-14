import { describe, expect, it } from 'vitest'
import { parseCalendarDate } from '../../lib/calendarDate'
import {
  cannotPriceReason,
  emptyStatementFigures,
  parseResalePriceStatementLetterParams,
  resalePriceStatementLetter,
  resalePriceStatementLetterPath,
  statementFiguresComplete,
  statementWorking,
  type StatementLease,
  type StatementPrice,
} from './resalePriceStatementLetter'

const d = parseCalendarDate

const appraisal: StatementLease = {
  homeownerFullName: 'Grace Whitaker',
  basePrice: 180_000,
  originalAppraisedValue: 260_000,
  formulaKind: 'AppraisalBased',
  appreciationShare: 0.25,
  improvementsCap: null,
}
const indexed: StatementLease = {
  homeownerFullName: 'Grace Whitaker',
  basePrice: 180_000,
  formulaKind: 'IndexBased',
  indexType: 'CPI',
  baselineIndexValue: 290,
  improvementsCap: 10_000,
}
const fixed: StatementLease = { homeownerFullName: 'Grace Whitaker', basePrice: 180_000, formulaKind: 'FixedPercentage', annualPercentage: 0.02 }

const price = (p: Partial<StatementPrice>): StatementPrice => ({ statementDate: d('2026-11-13'), yearsOwned: 6, price: 0, ...p })

describe('statementWorking', () => {
  it('shows the share of the rise in market value with its source', () => {
    expect(statementWorking(appraisal, price({ marketValue: 320_000, marketValueSource: 'County assessment 2026', price: 195_000 })))
      .toEqual(['Purchase price $180,000.00 plus 25% of the rise in market value: market value $320,000.00 (County assessment 2026) less $260,000.00 at purchase = $195,000.00'])
  })

  it('says fall when the market value is below the value at purchase', () => {
    expect(statementWorking(appraisal, price({ marketValue: 220_000, marketValueSource: 'Broker opinion', price: 170_000 }))[0])
      .toContain('less 25% of the fall in market value')
  })

  it('shows the index on the statement date against the index at purchase, then the credit on its own line', () => {
    expect(statementWorking(indexed, price({ indexValue: 312.5, improvementsCredit: 4_000, price: 197_965.52 }))).toEqual([
      'Purchase price $180,000.00 × CPI on November 13, 2026 312.5 ÷ CPI at purchase 290.0 = $193,965.52',
      'Approved improvements credit: $4,000.00',
    ])
  })

  it('shows the yearly rate and the full years owned', () => {
    expect(statementWorking(fixed, price({ price: 202_709.24 })))
      .toEqual(['Purchase price $180,000.00 plus 2% a year for 6 full years = $202,709.24'])
  })
})

describe('resalePriceStatementLetter', () => {
  it('states the price on the statement date and that it is not an offer', () => {
    const letter = resalePriceStatementLetter(fixed, '12 Elm St', price({ price: 202_709.24 }), 'Jo Steward\nCommon Ground CLT')
    expect(letter.recipient).toEqual(['Grace Whitaker', '12 Elm St'])
    expect(letter.priceSentence).toBe('Under your ground lease, the most your home can sell for on November 13, 2026 is $202,709.24.')
    expect(letter.closing).toContain('is not an offer')
    expect(letter.signOff).toEqual(['Jo Steward', 'Common Ground CLT'])
  })
})

describe('statementFiguresComplete', () => {
  it('needs a market value and source for an appraisal-based lease', () => {
    expect(statementFiguresComplete(appraisal, { ...emptyStatementFigures(), marketValue: 300_000 })).toBe(false)
    expect(statementFiguresComplete(appraisal, { ...emptyStatementFigures(), marketValue: 300_000, marketValueSource: 'County' })).toBe(true)
  })

  it('is never complete for an appraisal-based lease with no original appraised value', () => {
    const noOriginal = { ...appraisal, originalAppraisedValue: null }
    expect(cannotPriceReason(noOriginal)).not.toBeNull()
    expect(statementFiguresComplete(noOriginal, { ...emptyStatementFigures(), marketValue: 300_000, marketValueSource: 'County' })).toBe(false)
  })

  it('needs nothing for a fixed-rate lease', () => {
    expect(statementFiguresComplete(fixed, emptyStatementFigures())).toBe(true)
  })
})

describe('letter path', () => {
  it('round-trips the statement date and figures through the address', () => {
    const figures = { marketValue: 320_000, marketValueSource: 'County assessment 2026', indexValue: null, improvementsCredit: 4_000 }
    const path = resalePriceStatementLetterPath('lease-1', d('2026-11-13'), figures)
    expect(path.startsWith('/ground-leases/lease-1/resale-price-statement?')).toBe(true)
    expect(parseResalePriceStatementLetterParams(new URLSearchParams(path.split('?')[1])))
      .toEqual({ statementDate: d('2026-11-13'), figures })
  })

  it('gives no date when the address has a bad one', () => {
    expect(parseResalePriceStatementLetterParams(new URLSearchParams('date=nope')).statementDate).toBeNull()
  })
})
