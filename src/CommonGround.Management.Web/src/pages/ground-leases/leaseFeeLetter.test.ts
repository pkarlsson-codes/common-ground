import { describe, expect, it } from 'vitest'
import { parseCalendarDate } from '../../lib/calendarDate'
import { leaseFeeLetter, unpaidMonths } from './leaseFeeLetter'

const d = parseCalendarDate

describe('unpaidMonths', () => {
  it('names one month when only this month is unpaid', () => {
    expect(unpaidMonths(d('2026-11-01'), d('2026-11-20'))).toBe('November 2026')
  })

  it('names the range from the oldest unpaid month to this month', () => {
    expect(unpaidMonths(d('2026-09-01'), d('2026-11-20'))).toBe('September 2026 to November 2026')
  })
})

describe('leaseFeeLetter', () => {
  const lease = { homeownerFullName: 'Grace Whitaker', homeLabel: '12 Civic Way, Unit 001', monthlyFee: 60, balance: 180, oldestUnpaidDueDate: d('2026-09-01') }

  it('writes a first notice with the amount, the months and the sign-off lines', () => {
    const letter = leaseFeeLetter(lease, 'First', d('2026-11-10'), 'Riverbend CLT\n  (503) 555-0100 \n')

    expect(letter).toMatchObject({
      date: 'November 10, 2026',
      recipient: ['Grace Whitaker', '12 Civic Way, Unit 001'],
      heading: 'Lease fee past due',
      signOff: ['Riverbend CLT', '(503) 555-0100'],
    })
    expect(letter.paragraphs[0]).toBe(
      'Our records show that your lease fee has not been paid in full. As of November 10, 2026, you owe $180.00, for the monthly lease fee of $60.00 due for September 2026 to November 2026.')
    expect(letter.paragraphs.join(' ')).not.toContain('second notice')
  })

  it('warns of a notice of default in a second notice', () => {
    const letter = leaseFeeLetter(lease, 'Second', d('2026-11-10'), '')

    expect(letter.heading).toBe('Second notice: lease fee past due')
    expect(letter.paragraphs.join(' ')).toContain('formal notice of default')
    expect(letter.signOff).toEqual([])
  })

  it('gives a notice of default the cure deadline, the two-thirds extension and what happens if it is not cured', () => {
    const letter = leaseFeeLetter(lease, 'Default', d('2026-11-10'), '', { cureDays: 30, extensionDays: 30 })

    expect(letter.heading).toBe('Notice of default: lease fee unpaid')
    expect(letter.paragraphs).toContain(
      'This is a formal notice of default under your ground lease. To cure the default, you must pay the $180.00 owed within 30 days of the date of this notice, by December 10, 2026.')
    expect(letter.paragraphs).toContain(
      'If you pay at least two-thirds of this amount ($120.00) by December 10, 2026, you will have 30 more days, until January 9, 2027, to pay the rest.')
    expect(letter.paragraphs.join(' ')).toContain('may end your ground lease')
    expect(letter.paragraphs.join(' ')).not.toContain('disregard this letter')
  })

  it('leaves out the extension when the trust gives none', () => {
    const letter = leaseFeeLetter(lease, 'Default', d('2026-11-10'), '', { cureDays: 20, extensionDays: 0 })

    expect(letter.paragraphs.join(' ')).toContain('within 20 days of the date of this notice, by November 30, 2026.')
    expect(letter.paragraphs.join(' ')).not.toContain('two-thirds')
  })

  it('tells a first notice the interest rate and the day it is waived until', () => {
    const letter = leaseFeeLetter(
      { ...lease, balance: 60, oldestUnpaidDueDate: d('2026-11-01'), lateInterestRate: 10, interestWaiverDays: 30, interestWaivedIfPaidBy: d('2026-12-01') },
      'First', d('2026-11-10'), '')

    expect(letter.paragraphs[1]).toBe(
      'Under your ground lease, interest at 10% a year is counted on a late fee from the day it was due. If you pay the November 2026 fee by December 1, 2026, no interest will be charged on it.')
  })

  it('shows the interest owed as part of the amount in a second notice', () => {
    const letter = leaseFeeLetter({ ...lease, balance: 181.25, lateInterestRate: 10, interestWaiverDays: 30, interestDue: 1.25 }, 'Second', d('2026-11-10'), '')

    expect(letter.paragraphs[0]).toBe(
      'Our records show that your lease fee has not been paid in full. As of November 10, 2026, you owe $181.25, for the monthly lease fee of $60.00 due for September 2026 to November 2026. This includes $1.25 of interest at 10% a year on fees not paid within 30 days of their due date.')
  })

  it('says nothing about interest when the lease charges none', () => {
    const letter = leaseFeeLetter(lease, 'Second', d('2026-11-10'), '')

    expect(letter.paragraphs.join(' ')).not.toContain('interest')
  })
})
