import { describe, expect, it } from 'vitest'
import { FOCUSES, NAV_ITEMS, splitWork, type Focus } from './navigation'

const labels = (items: { label: string }[]) => items.map(i => i.label)

describe('splitWork', () => {
  it('puts every Work item under My work for Everything', () => {
    const { mine, other } = splitWork(NAV_ITEMS, 'everything')

    expect(labels(mine)).toEqual(['Households', 'Applications', 'Waiting list', 'Home selections', 'Purchases', 'Stewardship'])
    expect(other).toEqual([])
  })

  it.each<[Focus, string[], string[]]>([
    ['intake', ['Households', 'Applications', 'Waiting list'], ['Home selections', 'Purchases', 'Stewardship']],
    ['sales', ['Home selections', 'Purchases'], ['Households', 'Applications', 'Waiting list', 'Stewardship']],
    ['stewardship', ['Stewardship'], ['Households', 'Applications', 'Waiting list', 'Home selections', 'Purchases']],
  ])('splits Work for %s, keeping process order', (focus, mine, other) => {
    const split = splitWork(NAV_ITEMS, focus)

    expect(labels(split.mine)).toEqual(mine)
    expect(labels(split.other)).toEqual(other)
  })

  it.each(FOCUSES.map(f => f.value))('never moves Records or Settings for %s', (focus) => {
    const { mine, other } = splitWork(NAV_ITEMS, focus)

    expect([...mine, ...other].every(i => i.group === 'work')).toBe(true)
  })
})
