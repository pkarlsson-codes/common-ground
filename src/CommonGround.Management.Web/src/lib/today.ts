import type { WorkCountResponse, WorkItemResponse } from '../api'
import { FOCUS_AREAS, type Focus } from './navigation'

/** The areas Today groups items into, as the API names them. Reminders and Records sit outside any focus. */
export type TodayArea = 'reminders' | 'applicants' | 'selections' | 'purchases' | 'stewardship' | 'records'

/** The kind of a reminder's row. A reminder shows whatever the focus, in the section it was set in. */
export const REMINDER_KIND = 'reminder'

/** The action that marks a reminder done, straight from its row. */
export const MARK_REMINDER_DONE = 'mark-reminder-done'

export interface TodaySection {
  area: TodayArea
  label: string
}

/** Today's sections, in work order. */
export const TODAY_SECTIONS: readonly TodaySection[] = [
  { area: 'reminders', label: 'Reminders' },
  { area: 'applicants', label: 'Applicants' },
  { area: 'selections', label: 'Selections' },
  { area: 'purchases', label: 'Purchases' },
  { area: 'stewardship', label: 'Stewardship' },
  { area: 'records', label: 'Records' },
]

/** The sections a reminder can be set in: every Today section but Reminders, which holds those set in none. */
export const REMINDER_SECTIONS: readonly TodaySection[] = TODAY_SECTIONS.filter(s => s.area !== 'reminders')

/** The name of the Today section a reminder shows in. */
export function reminderSectionLabel(section: string | null | undefined): string {
  return TODAY_SECTIONS.find(s => s.area === (section ?? 'reminders'))?.label ?? section ?? 'Reminders'
}

/** The areas Today shows for a focus: everything, or the focus's own areas plus Reminders and Records. */
export function areasForFocus(focus: Focus): readonly TodayArea[] {
  if (focus === 'everything') return TODAY_SECTIONS.map(s => s.area)
  const areas = FOCUS_AREAS[focus] as readonly string[]
  return TODAY_SECTIONS.map(s => s.area).filter(a => a === 'reminders' || a === 'records' || areas.includes(a))
}

/**
 * The items Today shows for the areas shown: those in the areas, and every reminder, since a reminder belongs to the
 * whole trust.
 */
export function shownItems<T extends Pick<WorkItemResponse, 'area' | 'kind'>>(items: readonly T[], areas: readonly TodayArea[]): T[] {
  return items.filter(i => i.kind === REMINDER_KIND || (areas as readonly string[]).includes(i.area ?? ''))
}

export interface RecordLink {
  to: string
  /** Asks the page to open the record's detail panel, or (`enter`) its panel for entering a new record. */
  state?: { openId?: string; enter?: boolean }
}

const RECORD_PAGES: Record<string, string> = {
  application: '/applications',
  household: '/households',
  home: '/homes',
  purchase: '/purchase-workflows',
  document: '/documents',
  'ground-lease': '/ground-leases',
  resale: '/resales',
}

/** Where a row leads: the record's detail, opened on its list page. */
export function recordLink(item: Pick<WorkItemResponse, 'recordType' | 'recordId'>): RecordLink | null {
  const id = item.recordId
  if (item.recordType === 'income-limits') return { to: '/income-limits', state: id ? { openId: id } : undefined }
  if (!id) return null
  const to = RECORD_PAGES[item.recordType ?? '']
  return to ? { to, state: { openId: id } } : null
}

/** The sidebar page each kind of item is worked from, and the Today section it appears in. */
const KINDS: Record<string, { page: string; area: TodayArea }> = {
  // A reminder's own section varies; it counts under Reminders, which every focus shows.
  reminder: { page: '/reminders', area: 'reminders' },
  'application-awaiting-decision': { page: '/applications', area: 'applicants' },
  'readiness-step-missing': { page: '/households', area: 'applicants' },
  'prequalification-expiring': { page: '/households', area: 'applicants' },
  'home-available': { page: '/homes', area: 'selections' },
  'purchase-step-waiting': { page: '/purchase-workflows', area: 'purchases' },
  'purchase-stalled': { page: '/purchase-workflows', area: 'purchases' },
  'closing-scheduled': { page: '/purchase-workflows', area: 'purchases' },
  'lease-fee-notice-due': { page: '/stewardship', area: 'stewardship' },
  'lease-default-uncured': { page: '/stewardship', area: 'stewardship' },
  'mortgage-request-pending': { page: '/ground-leases', area: 'stewardship' },
  'mortgage-loan-closing-unrecorded': { page: '/ground-leases', area: 'stewardship' },
  'resale-price-statement-due': { page: '/stewardship', area: 'stewardship' },
  'resale-decision-due': { page: '/resales', area: 'stewardship' },
  'resale-closing-waiting': { page: '/resales', area: 'stewardship' },
  'insurance-lapsed': { page: '/ground-leases', area: 'stewardship' },
  'insurance-ending-soon': { page: '/ground-leases', area: 'stewardship' },
  'insurance-short-of-lease': { page: '/ground-leases', area: 'stewardship' },
  'document-expiring': { page: '/documents', area: 'records' },
  'income-limits-missing': { page: '/income-limits', area: 'selections' },
  'income-limits-stale': { page: '/income-limits', area: 'selections' },
}

/** How many items are waiting on each sidebar page, by its path. */
export function countsByPage(kindCounts: readonly WorkCountResponse[]): Record<string, number> {
  const counts: Record<string, number> = {}
  for (const { key, count } of kindCounts) {
    const page = KINDS[key ?? '']?.page
    if (page && count) counts[page] = (counts[page] ?? 0) + count
  }
  return counts
}

/** How many items the Today page shows under "My areas" for a focus. */
export function countForFocus(kindCounts: readonly WorkCountResponse[], focus: Focus): number {
  const areas = areasForFocus(focus)
  let total = 0
  for (const { key, count } of kindCounts) {
    const area = KINDS[key ?? '']?.area
    if (area && areas.includes(area)) total += count ?? 0
  }
  return total
}

const ACTION_LABELS: Record<string, string> = {
  'approve-application': 'Approve',
  'deny-application': 'Deny',
  'record-housing-counseling': 'Record counseling',
  'record-clt-orientation': 'Record orientation',
  'record-mortgage-prequalification': 'Record prequalification',
  'choose-household': 'Choose household',
  'record-eligibility': 'Record eligibility',
  'record-appraisal': 'Record appraisal',
  'record-financing': 'Record financing',
  'record-inspection': 'Record inspection',
  'record-title': 'Record title',
  'record-lease-execution': 'Record lease signing',
  'record-disclosure': 'Record disclosure',
  'schedule-closing': 'Schedule closing',
  'record-closing': 'Record closing',
  'record-lease-fee-notice': 'Record notice',
  'review-lease-default': 'Review default',
  'record-mortgage-request-appraisal': 'Add appraisal',
  'decide-mortgage-request': 'Decide',
  'decide-improvement-request': 'Decide',
  'record-mortgage-loan-closed': 'Confirm closing',
  'record-resale-price-statement': 'Prepare statement',
  'record-insurance-certificate': 'Record certificate',
  'record-resale-appraisal': 'Record appraisal',
  'exercise-purchase-option': 'Buy the home',
  'record-buyback-closing': 'Record buyback closing',
  'enter-income-limits': 'Enter limits',
  'mark-reminder-done': 'Mark done',
}

/** Actions that lead somewhere other than the row's record. */
const ACTION_LINKS: Record<string, RecordLink> = {
  'enter-income-limits': { to: '/income-limits', state: { enter: true } },
}

/** Where an action without a panel on Today leads: its own page, or else the row's record. */
export function actionLink(item: Pick<WorkItemResponse, 'recordType' | 'recordId'>, key: string): RecordLink | null {
  // A home's selection page, as homeSelectionPath gives it.
  if (key === 'choose-household' && item.recordType === 'home' && item.recordId) return { to: `/homes/${item.recordId}/selection` }
  return ACTION_LINKS[key] ?? recordLink(item)
}

/** The actions whose panel opens over Today; the rest open the record, where the action is. */
const PANEL_ACTIONS = new Set([
  'approve-application',
  'deny-application',
  'record-housing-counseling',
  'record-clt-orientation',
  'record-mortgage-prequalification',
  'record-mortgage-request-appraisal',
  'decide-mortgage-request',
  'decide-improvement-request',
  'record-mortgage-loan-closed',
  'record-resale-price-statement',
  'record-insurance-certificate',
  'record-resale-appraisal',
  'exercise-purchase-option',
  'record-buyback-closing',
])

export interface RowAction {
  key: string
  label: string
  /** Whether the action's panel opens over Today, rather than on the record's page. */
  opensPanel: boolean
}

/** The buttons a row offers, in the order the API gives; actions this app doesn't know are left out. */
export function rowActions(item: Pick<WorkItemResponse, 'actions'>): RowAction[] {
  return (item.actions ?? [])
    .filter(key => key in ACTION_LABELS)
    .map(key => ({ key, label: ACTION_LABELS[key], opensPanel: PANEL_ACTIONS.has(key) }))
}
