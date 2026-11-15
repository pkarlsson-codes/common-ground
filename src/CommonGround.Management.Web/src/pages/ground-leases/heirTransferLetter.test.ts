import { describe, expect, it } from 'vitest'
import type { HeirTransferResponse } from '../../api'
import { parseCalendarDate } from '../../lib/calendarDate'
import { heirLetterApplies, heirTransferLetter } from './heirTransferLetter'

const d = parseCalendarDate

const transfer: HeirTransferResponse = {
  id: 'heir-1',
  status: 'Completed',
  deceasedHomeownerFullName: 'Grace Whitaker',
  heirFullName: 'Tom Whitaker',
  transferredOn: d('2026-11-12'),
}

const letter = (t: HeirTransferResponse) =>
  heirTransferLetter(t, '12 Alder Way', d('2026-11-14'), 'Maria Lopez\nExecutive Director')

describe('heirTransferLetter', () => {
  it('consents under Art 10.3 and names the home, both people and the transfer date', () => {
    const text = letter(transfer).paragraphs.join(' ')
    expect(text).toContain('Art 10.3')
    expect(text).toContain('12 Alder Way')
    expect(text).toContain('Grace Whitaker')
    expect(text).toContain('November 12, 2026')
  })

  it('addresses the heir at the home and splits the sign-off', () => {
    const l = letter(transfer)
    expect(l.recipient).toEqual(['Tom Whitaker', '12 Alder Way'])
    expect(l.signOff).toEqual(['Maria Lopez', 'Executive Director'])
  })
})

describe('heirLetterApplies', () => {
  it.each([['Completed', true], ['Closed', false], ['Open', false], ['Heir recorded', false]])(
    '%s -> %s',
    (status, expected) => expect(heirLetterApplies(status)).toBe(expected),
  )
})
