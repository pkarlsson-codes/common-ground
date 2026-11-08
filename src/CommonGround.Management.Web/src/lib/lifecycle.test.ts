import { describe, expect, it } from 'vitest'
import { homeLabel, homeStage } from './lifecycle'

describe('homeLabel', () => {
  it.each<[string, string, string, string]>([
    ['Vacant', 'Offered', 'Available', 'Available'],
    ['Vacant', 'InSelection', 'InSelection', 'Selection'],
    ['Vacant', 'UnderContract', 'UnderContract', 'Purchasing'],
    ['Occupied', 'NotForSale', 'Leased', 'Leased'],
    ['Occupied', 'AwaitingTrustDecision', 'Resale', 'Resale'],
  ])('labels %s and %s as %s, in the %s stage', (occupancy, saleStage, label, stage) => {
    expect(homeLabel(occupancy, saleStage)).toBe(label)
    expect(homeStage(occupancy, saleStage)).toBe(stage)
  })

  it('shows both facts for a pair with no label', () => {
    expect(homeLabel('Occupied', 'Offered')).toBe('Occupied · Offered')
  })
})
