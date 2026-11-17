import { describe, expect, it } from 'vitest'
import { purchasePriceWarnings } from './purchasePriceCheck'

describe('purchasePriceWarnings', () => {
  it('warns when the price is above the prequalified amount', () => {
    const warnings = purchasePriceWarnings({ price: 250000, prequalifiedAmount: 210000 })
    expect(warnings).toHaveLength(1)
    expect(warnings[0]).toContain('$210,000')
  })

  it('does not warn when the price is at or below the prequalified amount', () => {
    expect(purchasePriceWarnings({ price: 210000, prequalifiedAmount: 210000 })).toEqual([])
  })

  it('skips the check when there is no prequalified amount, and when there is no price', () => {
    expect(purchasePriceWarnings({ price: 1, prequalifiedAmount: null })).toEqual([])
    expect(purchasePriceWarnings({ price: 0, prequalifiedAmount: 1 })).toEqual([])
  })
})
