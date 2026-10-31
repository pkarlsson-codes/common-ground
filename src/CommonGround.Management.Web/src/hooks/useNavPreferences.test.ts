import { act, renderHook } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'
import { readFocus, readOtherWorkOpen, useNavPreferences } from './useNavPreferences'

beforeEach(() => {
  window.localStorage.clear()
})

afterEach(() => {
  vi.restoreAllMocks()
})

describe('readFocus', () => {
  it('is Everything when nothing is stored', () => {
    expect(readFocus()).toBe('everything')
  })

  it('is Everything when the stored value is not a focus', () => {
    window.localStorage.setItem('cg.nav.focus', 'admin')

    expect(readFocus()).toBe('everything')
  })

  it('is Everything when storage throws', () => {
    vi.spyOn(Storage.prototype, 'getItem').mockImplementation(() => { throw new Error('blocked') })

    expect(readFocus()).toBe('everything')
  })
})

describe('readOtherWorkOpen', () => {
  it('is collapsed when nothing is stored', () => {
    expect(readOtherWorkOpen()).toBe(false)
  })
})

describe('useNavPreferences', () => {
  it('keeps a chosen focus and toggle for the next visit', () => {
    const { result } = renderHook(() => useNavPreferences())

    act(() => {
      result.current.setFocus('sales')
      result.current.setOtherWorkOpen(true)
    })

    expect(result.current.focus).toBe('sales')
    expect(readFocus()).toBe('sales')
    expect(readOtherWorkOpen()).toBe(true)
  })

  it('still changes the focus when storage cannot be written', () => {
    vi.spyOn(Storage.prototype, 'setItem').mockImplementation(() => { throw new Error('full') })
    const { result } = renderHook(() => useNavPreferences())

    act(() => result.current.setFocus('intake'))

    expect(result.current.focus).toBe('intake')
  })
})
