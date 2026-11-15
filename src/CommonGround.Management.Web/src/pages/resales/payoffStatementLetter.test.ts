import { describe, expect, it } from 'vitest'
import type { PayoffStatementResponse } from '../../api'
import { parseCalendarDate } from '../../lib/calendarDate'
import {
  goodThroughValid,
  parsePayoffStatementLetterParams,
  payoffStatementLetter,
  payoffStatementLetterPath,
  payoffWorking,
} from './payoffStatementLetter'

const d = parseCalendarDate

const owing: PayoffStatementResponse = {
  goodThrough: d('2026-11-20'),
  feesOwed: 180,
  interestOwed: 2.5,
  credit: 0,
  totalOwed: 182.5,
  oldestUnpaidMonth: d('2026-09-01'),
}
const paidUp: PayoffStatementResponse = { ...owing, feesOwed: 0, interestOwed: 0, totalOwed: 0, oldestUnpaidMonth: null }
const resale = { homeownerFullName: 'Grace Whitaker', homeLabel: '12 Elm St' }

describe('payoffStatementLetter', () => {
  it('asks the closing agent for the total as of the good-through day', () => {
    const letter = payoffStatementLetter(resale, owing, d('2026-11-15'), 'Pat Lee\nCommon Ground Trust')
    const text = letter.paragraphs.join(' ')
    expect(text).toContain('As of November 20, 2026, the amount owed is $182.50')
    expect(text).toContain('Please pay $182.50 to the trust at closing')
    expect(letter.recipient).toEqual(['Closing Agent', '12 Elm St'])
    expect(letter.signOff).toEqual(['Pat Lee', 'Common Ground Trust'])
  })

  it('says nothing is owed, and asks for no payment, when the lease is paid up', () => {
    const text = payoffStatementLetter(resale, paidUp, d('2026-11-15'), '').paragraphs.join(' ')
    expect(text).toContain('nothing is owed to the trust')
    expect(text).not.toContain('Please pay')
  })
})

describe('payoffWorking', () => {
  it('lists fees and interest, and adds credit and the oldest unpaid month only when there are any', () => {
    expect(payoffWorking(owing)).toEqual([
      'Lease fees owed: $180.00',
      'Interest owed: $2.50',
      'Oldest unpaid month: September 1, 2026',
    ])
    expect(payoffWorking({ ...paidUp, credit: 40 })).toEqual(['Lease fees owed: $0.00', 'Interest owed: $0.00', 'Credit: $40.00'])
  })
})

describe('goodThroughValid', () => {
  const today = d('2026-11-15')
  it('accepts today through 60 days ahead and nothing else', () => {
    expect(goodThroughValid(d('2026-11-15'), today)).toBe(true)
    expect(goodThroughValid(d('2027-01-14'), today)).toBe(true)
    expect(goodThroughValid(d('2027-01-15'), today)).toBe(false)
    expect(goodThroughValid(d('2026-11-14'), today)).toBe(false)
    expect(goodThroughValid(null, today)).toBe(false)
  })
})

describe('letter path', () => {
  it('round-trips the good-through day', () => {
    const path = payoffStatementLetterPath('resale-1', d('2026-11-20'))
    expect(path).toBe('/resales/resale-1/payoff-statement?goodThrough=2026-11-20')
    expect(parsePayoffStatementLetterParams(new URLSearchParams(path.split('?')[1]))).toBe('2026-11-20')
    expect(parsePayoffStatementLetterParams(new URLSearchParams('goodThrough=soon'))).toBeNull()
  })
})
