import { describe, expect, it } from 'vitest'
import type { Focus } from './navigation'
import { areasForFocus, countsByPage, recordLink, rowActions, type TodayArea } from './today'

describe('areasForFocus', () => {
  it.each<[Focus, TodayArea[]]>([
    ['everything', ['applicants', 'selections', 'purchases', 'records']],
    ['intake', ['applicants', 'records']],
    ['sales', ['selections', 'purchases', 'records']],
    ['stewardship', ['records']],
  ])('shows %s its own areas plus Records, in work order', (focus, areas) => {
    expect(areasForFocus(focus)).toEqual(areas)
  })
})

describe('recordLink', () => {
  it('opens a record on its list page', () => {
    expect(recordLink({ recordType: 'application', recordId: 'a-1' }))
      .toEqual({ to: '/applications', state: { openId: 'a-1' } })
  })

  it('opens a selection on its own page', () => {
    expect(recordLink({ recordType: 'selection', recordId: 's-1' })).toEqual({ to: '/homes/selections/s-1' })
  })

  it('has nowhere to go for a record type it does not know', () => {
    expect(recordLink({ recordType: 'mystery', recordId: 'm-1' })).toBeNull()
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
