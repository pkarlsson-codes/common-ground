import { describe, expect, it } from 'vitest'
import { HOME_STAGES, homeLabel, homeStage, homeStages } from './lifecycle'

describe('homeLabel', () => {
  it.each<[string, string, string, string]>([
    ['Vacant', 'Offered', 'Available', 'Available'],
    ['Vacant', 'InSelection', 'InSelection', 'Selection'],
    ['Vacant', 'UnderContract', 'UnderContract', 'Purchasing'],
    ['Occupied', 'NotForSale', 'Leased', 'Leased'],
    ['Occupied', 'AwaitingTrustDecision', 'Resale', 'Resale'],
    ['Vacant', 'HeldByTrust', 'HeldByTrust', 'Held by Trust'],
  ])('labels %s and %s as %s, in the %s stage', (occupancy, saleStage, label, stage) => {
    expect(homeLabel(occupancy, saleStage)).toBe(label)
    expect(homeStage(occupancy, saleStage)).toBe(stage)
  })

  it('shows both facts for a pair with no label', () => {
    expect(homeLabel('Occupied', 'Offered')).toBe('Occupied · Offered')
  })
})

describe('homeStages', () => {
  it('adds Held by Trust only for a home the trust is holding', () => {
    expect(homeStages('Resale')).toEqual(HOME_STAGES)
    expect(homeStages('Held by Trust')).toEqual([...HOME_STAGES, 'Held by Trust'])
  })
})
