import type { StewardshipCaseResponse } from '../../api/models/StewardshipCaseResponse'
import type { OccupancyVerificationResponse } from '../../api/models/OccupancyVerificationResponse'
import type { StewardshipQueueItemResponse } from '../../api/models/StewardshipQueueItemResponse'

export type UrgencyLevel = 'red' | 'orange' | 'yellow' | 'neutral'

/**
 * Classifies the urgency of a single stewardship case.
 *
 * Priority order (highest to lowest):
 *   1. red    — current-year occupancy verification is Failed or Overdue
 *   2. red    — any violation is NoticeIssued and cure deadline has passed
 *   3. orange — any violation is Identified (not yet notified)
 *   4. yellow — current-year occupancy verification is Pending and
 *               SurveyDeadline is within 30 days
 *   5. yellow — no occupancy verification exists for the current year
 *   6. neutral — everything else
 */
export function classifyUrgency(
  sc: StewardshipCaseResponse,
  today: Date = new Date(),
): UrgencyLevel {
  const currentYear = today.getFullYear()

  const currentVerification = (sc.occupancyVerifications ?? []).find(
    v => v.periodYear === currentYear,
  )

  // Rule 1: Failed or Overdue verification this year
  if (
    currentVerification &&
    (currentVerification.status === 'Failed' ||
      currentVerification.status === 'Overdue')
  ) {
    return 'red'
  }

  // Rule 2: NoticeIssued violation past cure deadline
  const hasOverdueCure = (sc.leaseViolations ?? []).some(v => {
    if (v.status !== 'NoticeIssued') return false
    if (!v.cureDeadline) return false
    return new Date(v.cureDeadline) < today
  })
  if (hasOverdueCure) return 'red'

  // Rule 3: Identified violation
  const hasIdentified = (sc.leaseViolations ?? []).some(v => v.status === 'Identified')
  if (hasIdentified) return 'orange'

  // Rule 4: Pending verification within 30 days
  if (currentVerification && currentVerification.status === 'Pending') {
    if (currentVerification.surveyDeadline) {
      const deadline = new Date(currentVerification.surveyDeadline)
      const msIn30Days = 30 * 24 * 60 * 60 * 1000
      if (deadline.getTime() - today.getTime() <= msIn30Days) {
        return 'yellow'
      }
    }
  }

  // Rule 5: No verification for current year at all
  if (!currentVerification) return 'yellow'

  return 'neutral'
}

/** Numeric weight for sorting: lower = more urgent (sorts to top). */
const URGENCY_WEIGHT: Record<UrgencyLevel, number> = {
  red: 0,
  orange: 1,
  yellow: 2,
  neutral: 3,
}

export const URGENCY_LABEL: Record<UrgencyLevel, string> = {
  red: 'Urgent',
  orange: 'High',
  yellow: 'Elevated',
  neutral: 'Normal',
}

export interface StewardshipQueueRow {
  caseId: string
  groundLeaseId: string
  homeownerName: string
  address: string
  currentVerification: OccupancyVerificationResponse | null
  activeViolationCount: number
  nextInspectionDate: Date | null
  openedDate: Date
  urgency: UrgencyLevel
}

/**
 * Turns the stewardship queue into rows sorted by urgency, then by open date.
 */
export function buildStewardshipQueue(
  items: StewardshipQueueItemResponse[],
  today: Date = new Date(),
): StewardshipQueueRow[] {
  const currentYear = today.getFullYear()

  const rows: StewardshipQueueRow[] = items.map(({ stewardshipCase: sc, homeownerFullName, address }) => {
    const currentVerification =
      (sc.occupancyVerifications ?? []).find(v => v.periodYear === currentYear) ?? null

    const activeViolationCount = (sc.leaseViolations ?? []).filter(
      v => v.status === 'Identified' || v.status === 'NoticeIssued',
    ).length

    // Next inspection = earliest Scheduled inspection date
    const scheduledDates = (sc.propertyInspections ?? [])
      .filter(i => i.status === 'Scheduled' && i.scheduledDate)
      .map(i => new Date(i.scheduledDate!))
      .sort((a, b) => a.getTime() - b.getTime())
    const nextInspectionDate = scheduledDates[0] ?? null

    return {
      caseId: sc.id!,
      groundLeaseId: sc.groundLeaseId!,
      homeownerName: homeownerFullName ?? '—',
      address: address ?? '—',
      currentVerification,
      activeViolationCount,
      nextInspectionDate,
      openedDate: sc.openedDate!,
      urgency: classifyUrgency(sc, today),
    }
  })

  // Stable sort: urgency weight first, then openedDate ascending
  rows.sort((a, b) => {
    const w = URGENCY_WEIGHT[a.urgency] - URGENCY_WEIGHT[b.urgency]
    if (w !== 0) return w
    return a.openedDate.getTime() - b.openedDate.getTime()
  })

  return rows
}
