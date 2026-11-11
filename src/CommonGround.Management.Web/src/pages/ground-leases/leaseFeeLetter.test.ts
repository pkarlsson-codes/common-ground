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
})
