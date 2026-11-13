import { describe, expect, it } from 'vitest'
import { parseAmount, parsePastedLimits } from './pastedLimits'

describe('parseAmount', () => {
  it('reads dollar signs and thousands separators', () => {
    expect(parseAmount('$50,050')).toBe(50050)
  })

  it('is NaN for words and empty text', () => {
    expect(parseAmount('Low')).toBeNaN()
    expect(parseAmount('')).toBeNaN()
  })
})

describe('parsePastedLimits', () => {
  it('reads tab-separated rows copied from a spreadsheet', () => {
    expect(parsePastedLimits('50\t34500\t39400\r\n80\t55200\t63050\r\n')).toEqual([
      { percentOfAmi: null, amounts: [50, 34500, 39400] },
      { percentOfAmi: null, amounts: [80, 55200, 63050] },
    ])
  })

  it('takes the percent of AMI from a HUD row label and drops its words', () => {
    expect(parsePastedLimits('Very Low (50%) Income Limits ($) $34,500 $39,400')).toEqual([
      { percentOfAmi: 50, amounts: [34500, 39400] },
    ])
  })

  it('reads a row whose label has no percent', () => {
    expect(parsePastedLimits('Extremely Low Income Limits ($)* 20,700 23,650')).toEqual([
      { percentOfAmi: null, amounts: [20700, 23650] },
    ])
  })

  it('leaves out blank lines, word-only lines and the household size header', () => {
    expect(parsePastedLimits('Persons in Family\n1 2 3 4 5 6 7 8\n\nLow (80%) 55,200')).toEqual([
      { percentOfAmi: 80, amounts: [55200] },
    ])
  })
})
