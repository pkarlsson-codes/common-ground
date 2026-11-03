import type { WorkCountResponse, WorkItemResponse } from '../api'
import { FOCUS_AREAS, type Focus } from './navigation'

/** The areas Today groups items into, as the API names them. Records sits outside any focus. */
export type TodayArea = 'applicants' | 'selections' | 'purchases' | 'records'

export interface TodaySection {
  area: TodayArea
  label: string
  /** The list page the section's "Open … →" link goes to. */
  to: string
  linkLabel: string
}

/** Today's sections, in work order. */
export const TODAY_SECTIONS: readonly TodaySection[] = [
  { area: 'applicants', label: 'Applicants', to: '/applications',       linkLabel: 'Open applicants' },
  { area: 'selections', label: 'Selections', to: '/home-selections',    linkLabel: 'Open selections' },
  { area: 'purchases',  label: 'Purchases',  to: '/purchase-workflows', linkLabel: 'Open purchases' },
  { area: 'records',    label: 'Records',    to: '/documents',          linkLabel: 'Open documents' },
]

/** The areas Today shows for a focus: everything, or the focus's own areas plus Records. */
export function areasForFocus(focus: Focus): readonly TodayArea[] {
  if (focus === 'everything') return TODAY_SECTIONS.map(s => s.area)
  const areas = FOCUS_AREAS[focus] as readonly string[]
  return TODAY_SECTIONS.map(s => s.area).filter(a => a === 'records' || areas.includes(a))
}

export interface RecordLink {
  to: string
  /** Asks the list page to open the record's detail panel. */
  state?: { openId: string }
}

const RECORD_PAGES: Record<string, string> = {
  application: '/applications',
  household: '/households',
  home: '/homes',
  purchase: '/purchase-workflows',
  document: '/documents',
}

/** Where a row leads: the record's detail, opened on its list page (selections have a page of their own). */
export function recordLink(item: Pick<WorkItemResponse, 'recordType' | 'recordId'>): RecordLink | null {
  const id = item.recordId
  if (!id) return null
  if (item.recordType === 'selection') return { to: `/homes/selections/${id}` }
  const to = RECORD_PAGES[item.recordType ?? '']
  return to ? { to, state: { openId: id } } : null
}

/** The sidebar page each kind of item is worked from. */
const KIND_PAGES: Record<string, string> = {
  'application-awaiting-decision': '/applications',
  'readiness-step-missing': '/households',
  'prequalification-expiring': '/households',
  'selection-open': '/home-selections',
  'home-available': '/homes',
  'purchase-step-waiting': '/purchase-workflows',
  'purchase-stalled': '/purchase-workflows',
  'closing-scheduled': '/purchase-workflows',
  'document-expiring': '/documents',
}

/** How many items are waiting on each sidebar page, by its path. */
export function countsByPage(kindCounts: readonly WorkCountResponse[]): Record<string, number> {
  const counts: Record<string, number> = {}
  for (const { key, count } of kindCounts) {
    const page = KIND_PAGES[key ?? '']
    if (page && count) counts[page] = (counts[page] ?? 0) + count
  }
  return counts
}
