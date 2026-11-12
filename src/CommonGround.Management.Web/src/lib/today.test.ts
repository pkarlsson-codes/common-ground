import { describe, expect, it } from 'vitest'
import type { Focus } from './navigation'
import { actionLink, areasForFocus, countForFocus, countsByPage, recordLink, rowActions, type TodayArea } from './today'

describe('areasForFocus', () => {
  it.each<[Focus, TodayArea[]]>([
    ['everything', ['applicants', 'selections', 'purchases', 'stewardship', 'records']],
    ['intake', ['applicants', 'records']],
    ['sales', ['selections', 'purchases', 'records']],
    ['stewardship', ['stewardship', 'records']],
  ])('shows %s its own areas plus Records, in work order', (focus, areas) => {
    expect(areasForFocus(focus)).toEqual(areas)
  })
})

describe('recordLink', () => {
  it('opens a record on its list page', () => {
    expect(recordLink({ recordType: 'application', recordId: 'a-1' }))
      .toEqual({ to: '/applications', state: { openId: 'a-1' } })
  })

  it('opens a ground lease on the Ground Leases page', () => {
    expect(recordLink({ recordType: 'ground-lease', recordId: 'gl-1' }))
      .toEqual({ to: '/ground-leases', state: { openId: 'gl-1' } })
  })

  it('opens a selection on its own page', () => {
    expect(recordLink({ recordType: 'selection', recordId: 's-1' })).toEqual({ to: '/homes/selections/s-1' })
  })

  it('has nowhere to go for a record type it does not know', () => {
    expect(recordLink({ recordType: 'mystery', recordId: 'm-1' })).toBeNull()
  })

  it('opens the Income Limits page, on the schedule in effect when there is one', () => {
    expect(recordLink({ recordType: 'income-limits', recordId: 'sch-1' }))
      .toEqual({ to: '/income-limits', state: { openId: 'sch-1' } })
    expect(recordLink({ recordType: 'income-limits', recordId: '' })).toEqual({ to: '/income-limits', state: undefined })
  })
})

describe('actionLink', () => {
  it('opens the Enter Income Limits panel for Enter limits', () => {
    expect(actionLink({ recordType: 'income-limits', recordId: 'sch-1' }, 'enter-income-limits'))
      .toEqual({ to: '/income-limits', state: { enter: true } })
  })

  it('opens the row record for other actions', () => {
    expect(actionLink({ recordType: 'home', recordId: 'h-1' }, 'start-selection'))
      .toEqual({ to: '/homes', state: { openId: 'h-1' } })
  })
})

describe('countsByPage', () => {
  it('adds up the kinds worked from the same page and skips unknown kinds', () => {
    expect(countsByPage([
      { key: 'readiness-step-missing', count: 2 },
      { key: 'prequalification-expiring', count: 1 },
      { key: 'closing-scheduled', count: 3 },
      { key: 'mystery', count: 9 },
    ])).toEqual({ '/households': 3, '/purchase-workflows': 3 })
  })
})

describe('countForFocus', () => {
  const kindCounts = [
    { key: 'readiness-step-missing', count: 2 },
    { key: 'home-available', count: 4 },
    { key: 'purchase-stalled', count: 3 },
    { key: 'document-expiring', count: 1 },
    { key: 'mystery', count: 9 },
  ]

  it.each<[Focus, number]>([
    ['everything', 10],
    ['intake', 3],
    ['sales', 8],
    ['stewardship', 1],
  ])('counts the kinds %s shows, plus Records', (focus, expected) => {
    expect(countForFocus(kindCounts, focus)).toBe(expected)
  })
})

describe('rowActions', () => {
  it('labels each action in the order given, saying which open over Today', () => {
    expect(rowActions({ actions: ['approve-application', 'start-selection'] })).toEqual([
      { key: 'approve-application', label: 'Approve', opensPanel: true },
      { key: 'start-selection', label: 'Start selection', opensPanel: false },
    ])
  })

  it('leaves out actions it does not know', () => {
    expect(rowActions({ actions: ['mystery', 'deny-application'] }).map(a => a.key)).toEqual(['deny-application'])
  })

  it('offers nothing when the item has no actions', () => {
    expect(rowActions({})).toEqual([])
  })
})
