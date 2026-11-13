import { render, screen } from '@testing-library/react'
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest'

vi.mock('./contexts/AuthContext', () => ({
  AuthProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  useAuth: vi.fn(),
}))
vi.mock('./hooks/demo', () => ({ useDemoMode: vi.fn() }))

import App from './App'
import { useAuth } from './contexts/AuthContext'
import { useDemoMode } from './hooks/demo'

const originalLocation = window.location
let redirectedTo: string | undefined

function signedOutAt(path: string) {
  window.history.pushState({}, '', path)
  vi.mocked(useAuth).mockReturnValue({ isLoading: false, isAuthenticated: false, user: null, error: null, logout: vi.fn(), refresh: vi.fn() })
  Object.defineProperty(window, 'location', {
    value: { ...originalLocation, pathname: path, search: '', hash: '', set href(url: string) { redirectedTo = url } },
    writable: true,
    configurable: true,
  })
}

function demoMode(data: { sandboxDays: number } | null, isLoading = false) {
  vi.mocked(useDemoMode).mockReturnValue({ data, isLoading } as unknown as ReturnType<typeof useDemoMode>)
}

describe('App', () => {
  beforeEach(() => { redirectedTo = undefined })
  afterEach(() => { Object.defineProperty(window, 'location', { value: originalLocation, writable: true, configurable: true }) })

  describe('signed-out visitor', () => {
    it('sees the Welcome page at / on a demo server', () => {
      signedOutAt('/')
      demoMode({ sandboxDays: 14 })

      render(<App />)

      expect(screen.getByRole('heading', { name: 'Try CommonGround with a sample trust' })).toBeTruthy()
      expect(screen.getByText(/deleted 14 days after you start it/)).toBeTruthy()
      expect(screen.getByRole('link', { name: 'Try the demo' }).getAttribute('href')).toBe('/auth/login?register=true')
      expect(screen.getByRole('link', { name: 'Sign in' }).getAttribute('href')).toBe('/auth/login')
      expect(redirectedTo).toBeUndefined()
    })

    it('waits for the demo check at / before redirecting', () => {
      signedOutAt('/')
      demoMode(undefined as unknown as null, true)

      render(<App />)

      expect(screen.getByRole('status', { name: 'Loading' })).toBeTruthy()
      expect(redirectedTo).toBeUndefined()
    })

    it('goes to sign-in at / when demo mode is off', () => {
      signedOutAt('/')
      demoMode(null)

      render(<App />)

      expect(redirectedTo).toBe('/auth/login?returnUrl=%2F')
    })

    it('goes to sign-in on any other path, even on a demo server', () => {
      signedOutAt('/land-parcels')
      demoMode(null)

      render(<App />)

      expect(vi.mocked(useDemoMode)).toHaveBeenLastCalledWith(false)
      expect(redirectedTo).toBe('/auth/login?returnUrl=%2Fland-parcels')
    })
  })
})
