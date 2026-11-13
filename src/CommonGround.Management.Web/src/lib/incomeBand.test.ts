import { describe, expect, it } from 'vitest'
import { incomeBandFor, incomeBandLabel } from './incomeBand'

const levels = [
  { percentOfAmi: 80, limits: [62_050, 70_900, 79_750, 88_600, 95_700, 102_800, 109_900, 116_950] },
  { percentOfAmi: 30, limits: [23_250, 26_600, 29_900, 33_200, 35_900, 38_550, 41_200, 43_850] },
  { percentOfAmi: 50, limits: [38_750, 44_300, 49_850, 55_350, 59_800, 64_250, 68_650, 73_100] },
]

describe('incomeBandLabel', () => {
  it.each<[number | null, number | null, string | null]>([
    [null, 30, 'Up to 30% of AMI'],
    [30, 50, 'Over 30% to 50% of AMI'],
    [120, null, 'Over 120% of AMI'],
    [null, null, null],
  ])('labels over %s up to %s as %s', (over, upTo, label) => {
    expect(incomeBandLabel({ overPercentOfAmi: over, upToPercentOfAmi: upTo })).toBe(label)
  })
})

describe('incomeBandFor', () => {
  it.each<[number, number, number | null, number | null]>([
    [29_900, 3, null, 30],
    [29_900.01, 3, 30, 50],
    [49_850, 3, 30, 50],
    [79_750.01, 3, 80, null],
  ])('puts %s for %s people over %s up to %s', (income, size, over, upTo) => {
    expect(incomeBandFor(levels, income, size)).toEqual({ overPercentOfAmi: over, upToPercentOfAmi: upTo })
  })

  it('leaves households above 8 people to the API', () => {
    expect(incomeBandFor(levels, 40_000, 9)).toBeNull()
  })
})
