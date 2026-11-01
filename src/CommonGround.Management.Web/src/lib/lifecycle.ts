import { formatCalendarDate, trustToday, type CalendarDate } from './calendarDate'
import type { HouseholdResponse } from '../api'

export type LifecycleStepState = 'done' | 'current' | 'todo'

export interface LifecycleStep {
  label: string
  /** When the step was reached, already formatted; omitted for steps not reached yet. */
  date?: string
  state: LifecycleStepState
}

/**
 * Builds the steps of a lifecycle from its stage names, marking those before `current` done, it current
 * and the rest to do. `dates` gives the date shown under each step reached.
 */
export function lifecycleSteps(stages: readonly string[], current: string, dates: Partial<Record<string, string | undefined>>): LifecycleStep[] {
  const currentIndex = stages.indexOf(current)
  return stages.map((label, i) => {
    const state: LifecycleStepState = i < currentIndex ? 'done' : i === currentIndex ? 'current' : 'todo'
    return { label, state, date: state === 'todo' ? undefined : dates[label] }
  })
}

/** Formats a step's date, whether a calendar date or an instant, as e.g. "Oct 28, 2026". */
export function formatStepDate(value?: CalendarDate | Date | null): string | undefined {
  if (!value) return undefined
  if (value instanceof Date) return value.toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })
  return formatCalendarDate(value)
}

// ── Homes ─────────────────────────────────────────────────────────────────

export const HOME_STAGES = ['Development', 'Available', 'Selection', 'Purchasing', 'Leased', 'Resale'] as const

const HOME_STAGE_OF_STATUS: Record<string, string> = {
  Available: 'Available',
  InSelection: 'Selection',
  UnderContract: 'Purchasing',
  Leased: 'Leased',
  Resale: 'Resale',
}

/** The home's lifecycle stage, from its status. */
export const homeStage = (status?: string | null) => HOME_STAGE_OF_STATUS[status ?? ''] ?? 'Available'

// ── Households ────────────────────────────────────────────────────────────

export const HOUSEHOLD_STAGES = ['Intake', 'Ready', 'Approved', 'Waiting', 'Selected', 'Purchasing', 'Homeowner'] as const

export type ReadinessItem = 'housing-counseling' | 'clt-orientation' | 'mortgage-prequalification'

export const READINESS_LABELS: Record<ReadinessItem, string> = {
  'housing-counseling': 'Housing Counseling',
  'clt-orientation': 'CLT Orientation',
  'mortgage-prequalification': 'Mortgage Prequalification',
}

/** The readiness items the household still has to record, including a prequalification that has expired. */
export function missingReadiness(household: HouseholdResponse): ReadinessItem[] {
  const missing: ReadinessItem[] = []
  if (!household.isHousingCounselingCompleted) missing.push('housing-counseling')
  if (!household.isCltOrientationCompleted) missing.push('clt-orientation')
  const prequalification = household.mortgagePrequalification
  if (!prequalification?.expiryDate || prequalification.expiryDate < trustToday()) missing.push('mortgage-prequalification')
  return missing
}
