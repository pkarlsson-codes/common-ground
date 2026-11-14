import { describe, expect, it } from 'vitest'
import { filledSources, marketValueLeases, roundFigures, roundIndexTypes, type RoundLease } from './resalePriceRound'

const lease = (id: string, change: Partial<RoundLease>): RoundLease => ({
  groundLeaseId: id,
  homeownerFullName: `Owner ${id}`,
  basePrice: 180_000,
  ...change,
})

const appraisalA = lease('a', { formulaKind: 'AppraisalBased', originalAppraisedValue: 260_000, appreciationShare: 0.25 })
const appraisalB = lease('b', { formulaKind: 'AppraisalBased', originalAppraisedValue: 240_000, appreciationShare: 0.25 })
const noAppraisal = lease('c', { formulaKind: 'AppraisalBased', originalAppraisedValue: null, appreciationShare: 0.25 })
const cpi1 = lease('d', { formulaKind: 'IndexBased', indexType: 'CPI-U', baselineIndexValue: 290 })
const cpi2 = lease('e', { formulaKind: 'IndexBased', indexType: 'CPI-U', baselineIndexValue: 301.5 })
const fixed = lease('f', { formulaKind: 'FixedPercentage', annualPercentage: 0.02 })

describe('roundIndexTypes', () => {
  it('asks once per index type, with each distinct baseline', () => {
    expect(roundIndexTypes([cpi1, appraisalA, cpi2, lease('g', { ...cpi1, groundLeaseId: 'g' })]))
      .toEqual([{ indexType: 'CPI-U', baselines: [290, 301.5] }])
  })
})

describe('marketValueLeases', () => {
  it('leaves out an appraisal-based lease with no original appraised value', () => {
    expect(marketValueLeases([appraisalA, noAppraisal, cpi1]).map(l => l.groundLeaseId)).toEqual(['a'])
  })
})

describe('filledSources', () => {
  it('fills a blank source down from the nearest row above', () => {
    expect(filledSources([appraisalA, appraisalB], {
      a: { value: 300_000, source: ' County 2026 ' },
      b: { value: 280_000, source: '' },
    })).toEqual({ a: 'County 2026', b: 'County 2026' })
  })

  it('leaves the first row blank when it has no source', () => {
    expect(filledSources([appraisalA, appraisalB], { b: { value: 1, source: 'Broker' } })).toEqual({ a: '', b: 'Broker' })
  })
})

describe('roundFigures', () => {
  it('gives each lease the figures its formula needs and leaves out leases still missing one', () => {
    const figures = roundFigures([appraisalA, appraisalB, noAppraisal, cpi1, fixed], {
      indexValues: { 'CPI-U': 319 },
      marketValues: { a: { value: 300_000, source: 'County 2026' } },
    })
    expect(figures).toEqual([
      { groundLeaseId: 'a', figures: { marketValue: 300_000, marketValueSource: 'County 2026', indexValue: null, improvementsCredit: null } },
      { groundLeaseId: 'd', figures: { marketValue: null, marketValueSource: '', indexValue: 319, improvementsCredit: null } },
      { groundLeaseId: 'f', figures: { marketValue: null, marketValueSource: '', indexValue: null, improvementsCredit: null } },
    ])
  })
})
