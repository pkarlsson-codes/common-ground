import { describe, expect, it } from 'vitest'
import type { ImprovementRequestResponse } from '../../api'
import { parseCalendarDate } from '../../lib/calendarDate'
import { improvementLetterApplies, improvementRequestLetter } from './improvementRequestLetter'

const d = parseCalendarDate

const request: ImprovementRequestResponse = {
  id: 'imp-1',
  homeownerFullName: 'Grace Whitaker',
  status: 'Approved',
  receivedOn: d('2026-11-02'),
  description: 'Replace the roof',
  estimatedCost: 18_000,
  preApprovedCredit: 15_000,
  denialReason: null,
}

const letter = (r: ImprovementRequestResponse) =>
  improvementRequestLetter(r, '12 Alder Way', d('2026-11-10'), 'Maria Lopez\nExecutive Director')

describe('improvementRequestLetter', () => {
  it('approval names the work, the credit and that receipts must come first', () => {
    const text = letter(request).paragraphs.join(' ')
    expect(text).toContain('Replace the roof')
    expect(text).toContain('$15,000.00')
    expect(text).toContain('receipts')
  })

  it('approval with a zero credit says no credit and asks for no receipts', () => {
    const text = letter({ ...request, preApprovedCredit: 0 }).paragraphs.join(' ')
    expect(text).toContain('without a credit')
    expect(text).not.toContain('receipts')
  })

  it('denial carries the reason', () => {
    const l = letter({ ...request, status: 'Denied', denialReason: 'The addition crosses the setback.' })
    expect(l.heading).toBe('Improvements to your home not approved')
    expect(l.paragraphs).toContain('The addition crosses the setback.')
  })

  it('addresses the homeowner at the home and splits the sign-off', () => {
    const l = letter(request)
    expect(l.recipient).toEqual(['Grace Whitaker', '12 Alder Way'])
    expect(l.signOff).toEqual(['Maria Lopez', 'Executive Director'])
  })
})

describe('improvementLetterApplies', () => {
  it.each([['Approved', true], ['Denied', true], ['Completed', true], ['Withdrawn', false], ['Waiting for decision', false]])(
    '%s -> %s',
    (status, expected) => expect(improvementLetterApplies(status)).toBe(expected),
  )
})
