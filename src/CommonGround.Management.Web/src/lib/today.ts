import type { WorkCountResponse, WorkItemResponse } from '../api'
import { FOCUS_AREAS, type Focus } from './navigation'

/** The areas Today groups items into, as the API names them. Records sits outside any focus. */
export type TodayArea = 'applicants' | 'selections' | 'purchases' | 'stewardship' | 'records'

export interface TodaySection {
  area: TodayArea
  label: string
}

/** Today's sections, in work order. */
export const TODAY_SECTIONS: readonly TodaySection[] = [
  { area: 'applicants', label: 'Applicants' },
  { area: 'selections', label: 'Selections' },
  { area: 'purchases', label: 'Purchases' },
  { area: 'stewardship', label: 'Stewardship' },
  { area: 'records', label: 'Records' },
]

/** The areas Today shows for a focus: everything, or the focus's own areas plus Records. */
export function areasForFocus(focus: Focus): readonly TodayArea[] {
  if (focus === 'everything') return TODAY_SECTIONS.map(s => s.area)
  const areas = FOCUS_AREAS[focus] as readonly string[]
  return TODAY_SECTIONS.map(s => s.area).filter(a => a === 'records' || areas.includes(a))
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
}

/** Where a row leads: the record's detail, opened on its list page (selections have a page of their own). */
export function recordLink(item: Pick<WorkItemResponse, 'recordType' | 'recordId'>): RecordLink | null {
  const id = item.recordId
  if (item.recordType === 'income-limits') return { to: '/income-limits', state: id ? { openId: id } : undefined }
  if (!id) return null
  if (item.recordType === 'selection') return { to: `/homes/selections/${id}` }
  const to = RECORD_PAGES[item.recordType ?? '']
  return to ? { to, state: { openId: id } } : null
}

/** The sidebar page each kind of item is worked from, and the Today section it appears in. */
const KINDS: Record<string, { page: string; area: TodayArea }> = {
  'application-awaiting-decision': { page: '/applications', area: 'applicants' },
  'readiness-step-missing': { page: '/households', area: 'applicants' },
  'prequalification-expiring': { page: '/households', area: 'applicants' },
  'selection-open': { page: '/home-selections', area: 'selections' },
  'home-available': { page: '/homes', area: 'selections' },
  'purchase-step-waiting': { page: '/purchase-workflows', area: 'purchases' },
  'purchase-stalled': { page: '/purchase-workflows', area: 'purchases' },
  'closing-scheduled': { page: '/purchase-workflows', area: 'purchases' },
  'lease-fee-notice-due': { page: '/stewardship', area: 'stewardship' },
  'lease-default-uncured': { page: '/stewardship', area: 'stewardship' },
  'mortgage-request-pending': { page: '/ground-leases', area: 'stewardship' },
  'mortgage-loan-closing-unrecorded': { page: '/ground-leases', area: 'stewardship' },
  'resale-price-statement-due': { page: '/stewardship', area: 'stewardship' },
  'insurance-certificate-due': { page: '/ground-leases', area: 'stewardship' },
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
  'start-selection': 'Start selection',
  'choose-household': 'Choose household',
  'record-eligibility': 'Record eligibility',
  'record-appraisal': 'Record appraisal',
  'record-financing': 'Record financing',
  'record-inspection': 'Record inspection',
  'record-title': 'Record title',
  'record-lease-execution': 'Record lease signing',
  'schedule-closing': 'Schedule closing',
  'record-closing': 'Record closing',
  'record-lease-fee-notice': 'Record notice',
  'review-lease-default': 'Review default',
  'record-mortgage-request-appraisal': 'Add appraisal',
  'decide-mortgage-request': 'Decide',
  'record-mortgage-loan-closed': 'Confirm closing',
  'record-resale-price-statement': 'Prepare statement',
  'record-insurance-certificate': 'Record certificate',
  'enter-income-limits': 'Enter limits',
}

/** Actions that lead somewhere other than the row's record. */
const ACTION_LINKS: Record<string, RecordLink> = {
  'enter-income-limits': { to: '/income-limits', state: { enter: true } },
}

/** Where an action without a panel on Today leads: its own page, or else the row's record. */
export function actionLink(item: Pick<WorkItemResponse, 'recordType' | 'recordId'>, key: string): RecordLink | null {
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
  'record-mortgage-loan-closed',
  'record-resale-price-statement',
  'record-insurance-certificate',
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
