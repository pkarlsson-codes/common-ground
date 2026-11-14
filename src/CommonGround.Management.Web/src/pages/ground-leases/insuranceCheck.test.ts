import { describe, expect, it } from 'vitest'
import type { InsuranceRequirementsChangeDto } from '../../api'
import { parseCalendarDate } from '../../lib/calendarDate'
import { checkCertificate, requirementsOn } from './insuranceCheck'

const d = parseCalendarDate
const change = (effectiveFrom: string, replacementValue: number | null, liabilityPerOccurrence: number | null): InsuranceRequirementsChangeDto =>
  ({ effectiveFrom: d(effectiveFrom), replacementValue, liabilityPerOccurrence })

describe('requirementsOn', () => {
  // Latest effective first, as the lease view gives it.
  const history = [change('2026-06-01', 300_000, 500_000), change('2025-01-01', 280_000, 300_000)]

  it('takes the latest change effective on or before the day', () => {
    expect(requirementsOn(history, d('2026-06-01'))).toEqual({ replacementValue: 300_000, liabilityPerOccurrence: 500_000 })
    expect(requirementsOn(history, d('2026-05-31'))).toEqual({ replacementValue: 280_000, liabilityPerOccurrence: 300_000 })
  })

  it('takes the first change for a day before the lease start', () => {
    expect(requirementsOn(history, d('2024-12-01'))).toEqual({ replacementValue: 280_000, liabilityPerOccurrence: 300_000 })
  })
})

describe('checkCertificate', () => {
  const requirements = { replacementValue: 280_000, liabilityPerOccurrence: 300_000 }

  it('finds no shortfall when every amount is met and the trust is named', () => {
    const check = checkCertificate(requirements, { dwellingCoverage: 280_000, liabilityPerOccurrence: 300_000, trustNamedAdditionalInsured: true })
    expect(check).toEqual({ shortfalls: [], notChecked: [] })
  })

  it('lists each shortfall', () => {
    const check = checkCertificate(requirements, { dwellingCoverage: 250_000, liabilityPerOccurrence: 100_000, trustNamedAdditionalInsured: false })
    expect(check.shortfalls).toEqual([
      'Dwelling coverage $250,000.00 is below the replacement value $280,000.00',
      'Liability $100,000.00 is below the $300,000.00 required',
      'The trust is not named as additional insured',
    ])
  })

  it('reports a requirement that is not set as not checked, not as a shortfall', () => {
    const check = checkCertificate(
      { replacementValue: null, liabilityPerOccurrence: null },
      { dwellingCoverage: 0, liabilityPerOccurrence: 0, trustNamedAdditionalInsured: true },
    )
    expect(check.shortfalls).toEqual([])
    expect(check.notChecked).toHaveLength(2)
  })
})
