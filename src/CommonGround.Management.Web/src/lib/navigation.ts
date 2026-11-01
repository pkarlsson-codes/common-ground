import {
  ClipboardList,
  FileCog,
  FolderOpen,
  Hammer,
  HeartHandshake,
  House,
  KeyRound,
  ListChecks,
  ListOrdered,
  Map as MapIcon,
  Repeat,
  ScrollText,
  Users,
  type LucideIcon,
} from 'lucide-react'

/** The part of the CLT's work a page belongs to. Records and settings have none. */
export type WorkArea = 'applicants' | 'selections' | 'purchases' | 'stewardship' | 'resales'

export type NavGroup = 'work' | 'records' | 'settings'

export interface NavItem {
  label: string
  to: string
  icon: LucideIcon
  group: NavGroup
  area?: WorkArea
}

/** Every sidebar item, Work in the order the process runs. */
export const NAV_ITEMS: readonly NavItem[] = [
  { label: 'Households',      to: '/households',             icon: Users,          group: 'work', area: 'applicants' },
  { label: 'Applications',    to: '/applications',           icon: ClipboardList,  group: 'work', area: 'applicants' },
  { label: 'Waiting list',    to: '/waiting-list',           icon: ListOrdered,    group: 'work', area: 'applicants' },
  { label: 'Home selections', to: '/home-selections',        icon: ListChecks,     group: 'work', area: 'selections' },
  { label: 'Purchases',       to: '/purchase-workflows',     icon: KeyRound,       group: 'work', area: 'purchases' },
  { label: 'Stewardship',     to: '/stewardship',            icon: HeartHandshake, group: 'work', area: 'stewardship' },
  { label: 'Resales',         to: '/resale-workflows',       icon: Repeat,         group: 'work', area: 'resales' },
  { label: 'Land parcels',    to: '/land-parcels',           icon: MapIcon,        group: 'records' },
  { label: 'Development',     to: '/development',            icon: Hammer,         group: 'records' },
  { label: 'Homes',           to: '/homes',                  icon: House,          group: 'records' },
  { label: 'Ground leases',   to: '/ground-leases',          icon: ScrollText,     group: 'records' },
  { label: 'Documents',       to: '/documents',              icon: FolderOpen,     group: 'records' },
  { label: 'Lease templates', to: '/ground-lease-templates', icon: FileCog,        group: 'settings' },
]

export type Focus = 'everything' | 'intake' | 'sales' | 'stewardship'

export const FOCUSES: readonly { value: Focus; label: string }[] = [
  { value: 'everything',  label: 'Everything' },
  { value: 'intake',      label: 'Intake' },
  { value: 'sales',       label: 'Sales' },
  { value: 'stewardship', label: 'Stewardship' },
]

const FOCUS_AREAS: Record<Exclude<Focus, 'everything'>, readonly WorkArea[]> = {
  intake:      ['applicants'],
  sales:       ['selections', 'purchases', 'resales'],
  stewardship: ['stewardship'],
}

export function isFocus(value: unknown): value is Focus {
  return FOCUSES.some(f => f.value === value)
}

/** Splits the Work items into those in the focus and the rest, keeping process order. */
export function splitWork(items: readonly NavItem[], focus: Focus): { mine: NavItem[]; other: NavItem[] } {
  const work = items.filter(i => i.group === 'work')
  if (focus === 'everything') return { mine: work, other: [] }

  const areas = FOCUS_AREAS[focus]
  return {
    mine: work.filter(i => i.area !== undefined && areas.includes(i.area)),
    other: work.filter(i => i.area === undefined || !areas.includes(i.area)),
  }
}

/** Where the app's root lands: the first page under My work. */
export function landingPath(focus: Focus): string {
  return splitWork(NAV_ITEMS, focus).mine[0].to
}
