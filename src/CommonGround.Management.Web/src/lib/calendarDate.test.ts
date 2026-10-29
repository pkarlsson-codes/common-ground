import { afterEach, describe, expect, it, vi } from 'vitest'
import {
  addDays,
  daysBetween,
  calendarDateOrNull,
  formatCalendarDate,
  isCalendarDate,
  parseCalendarDate,
  setTrustTimeZone,
  todayIn,
  trustToday,
} from './calendarDate'

// 03:00 UTC on Oct 29 is the evening of Oct 28 in Denver.
const eveningInDenver = new Date('2026-10-29T03:00:00Z')

describe('parseCalendarDate', () => {
  it('accepts a real YYYY-MM-DD date', () => {
    expect(parseCalendarDate('2026-02-28')).toBe('2026-02-28')
  })

  it.each(['2026-02-30', '2026-2-28', '2026-02-28T00:00:00Z', ''])('rejects %j', (value) => {
    expect(isCalendarDate(value)).toBe(false)
    expect(() => parseCalendarDate(value)).toThrow()
  })
})

describe('calendarDateOrNull', () => {
  it('maps an empty date input to null', () => {
    expect(calendarDateOrNull('')).toBeNull()
  })
})

describe('addDays', () => {
  it('crosses month and year ends', () => {
    expect(addDays(parseCalendarDate('2026-12-31'), 1)).toBe('2027-01-01')
    expect(addDays(parseCalendarDate('2026-03-01'), -1)).toBe('2026-02-28')
  })

  it('is not shifted by daylight saving changes', () => {
    expect(addDays(parseCalendarDate('2026-11-01'), 1)).toBe('2026-11-02')
  })
})

describe('todayIn', () => {
  it('gives the date in the time zone, not the UTC date', () => {
    expect(todayIn('America/Denver', eveningInDenver)).toBe('2026-10-28')
    expect(todayIn('UTC', eveningInDenver)).toBe('2026-10-29')
  })
})

describe('trustToday', () => {
  afterEach(() => {
    setTrustTimeZone(undefined)
    vi.useRealTimers()
  })

  it("follows the trust's time zone", () => {
    vi.useFakeTimers()
    vi.setSystemTime(eveningInDenver)
    setTrustTimeZone('America/Denver')

    expect(trustToday()).toBe('2026-10-28')
    expect(trustToday(30)).toBe('2026-11-27')
  })
})

describe('formatCalendarDate', () => {
  it('formats the date itself, whatever the browser time zone', () => {
    expect(formatCalendarDate(parseCalendarDate('2026-10-28'))).toBe('Oct 28, 2026')
  })
})

describe('daysBetween', () => {
  it('counts whole days across a daylight saving change', () => {
    expect(daysBetween(parseCalendarDate('2026-10-28'), parseCalendarDate('2026-11-04'))).toBe(7)
  })
})
