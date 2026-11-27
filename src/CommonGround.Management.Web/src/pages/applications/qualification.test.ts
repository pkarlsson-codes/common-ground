import { describe, expect, it } from 'vitest'
import { parseCalendarDate } from '../../lib/calendarDate'
import { qualificationChanges } from './qualification'

const since = parseCalendarDate('2026-10-28')

describe('qualificationChanges', () => {
  it('lists the income and size that changed since the last check', () => {
    expect(qualificationChanges({
      annualIncome: 61000,
      householdSize: 4,
      qualifiedAnnualIncome: 52000,
      qualifiedHouseholdSize: 3,
      qualificationToConfirmSince: since,
    })).toEqual(['Income $52,000 → $61,000', 'Household size 3 → 4'])
  })

  it('leaves out a figure that is back to the one checked', () => {
    expect(qualificationChanges({
      annualIncome: 52000,
      householdSize: 4,
      qualifiedAnnualIncome: 52000,
      qualifiedHouseholdSize: 3,
      qualificationToConfirmSince: since,
    })).toEqual(['Household size 3 → 4'])
  })

  it('is empty when there is nothing to confirm', () => {
    expect(qualificationChanges({ annualIncome: 61000, householdSize: 4, qualificationToConfirmSince: null })).toEqual([])
  })
})
