import { describe, expect, it } from 'vitest'
import type { MortgageRequestResponse } from '../../api'
import { parseCalendarDate } from '../../lib/calendarDate'
import { businessDaysBetween, debtCheckText, mortgageRequestLetter } from './mortgageRequestLetter'

const d = parseCalendarDate

const request: MortgageRequestResponse = {
  id: 'req-1',
  homeownerFullName: 'Grace Whitaker',
  status: 'Approved',
  receivedOn: d('2026-11-02'),
  lender: 'Columbia Bank',
  purpose: 'Refinance',
  principal: 175_000,
  totalDebtAfterClosing: 175_000,
  interestRatePercent: 6.25,
  rateType: 'Fixed',
  termYears: 30,
  appraisedValue: 300_000,
  purchaseOptionPrice: 220_000,
  maxMortgageDebtPercent: 90,
  debtCapAmount: 198_000,
  debtPercent: 79.5,
  overCap: false,
  decisionReason: null,
}

describe('businessDaysBetween', () => {
  it('counts weekdays after the first day up to and including the last', () => {
    // Monday Nov 2 to Monday Nov 9, 2026: Tue to Fri, then Mon.
    expect(businessDaysBetween(d('2026-11-02'), d('2026-11-09'))).toBe(5)
  })

  it('is zero for the same day', () => {
    expect(businessDaysBetween(d('2026-11-02'), d('2026-11-02'))).toBe(0)
  })
})

describe('debtCheckText', () => {
  it('is null until the appraisal is in', () => {
    expect(debtCheckText({ ...request, purchaseOptionPrice: null, debtCapAmount: null, debtPercent: null })).toBeNull()
  })

  it('says by how much the debt is above the cap', () => {
    expect(debtCheckText({ ...request, totalDebtAfterClosing: 246_400, debtPercent: 112, overCap: true })).toEqual({
      share: 'Total debt after closing is 112% of the purchase option price.',
      verdict: 'Above the cap by $48,400.00.',
      overCap: true,
    })
  })
})

describe('mortgageRequestLetter', () => {
  it('gives permission naming the terms and the cap figures, copied to the lender', () => {
    const letter = mortgageRequestLetter(request, '12 Civic Way, Unit 001', d('2026-11-10'), 'Riverbend CLT')

    expect(letter).toMatchObject({
      date: 'November 10, 2026',
      recipient: ['Grace Whitaker', '12 Civic Way, Unit 001'],
      heading: 'Permission for a mortgage loan',
      cc: 'cc: Columbia Bank',
    })
    expect(letter.paragraphs[0]).toContain('a principal of $175,000.00 at 6.25% fixed, 30 years')
    expect(letter.paragraphs[1]).toBe(
      'Based on the appraisal of $300,000.00, the purchase option price under your lease is $220,000.00. Your lease '
        + 'limits total mortgage debt to 90% of that price, $198,000.00. The total debt after this loan is 79.5% of the price.')
  })

  it('refuses with the reason when denied', () => {
    const letter = mortgageRequestLetter(
      { ...request, status: 'Denied', decisionReason: 'Total debt would be 112% of the price.' }, null, d('2026-11-10'), '')

    expect(letter.heading).toBe('Mortgage loan not permitted')
    expect(letter.recipient).toEqual(['Grace Whitaker'])
    expect(letter.paragraphs).toContain('Total debt would be 112% of the price.')
  })
})
