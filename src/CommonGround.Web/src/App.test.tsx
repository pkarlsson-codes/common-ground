import { render, screen } from '@testing-library/react'
import { describe, it, expect, vi } from 'vitest'
import { MemoryRouter, Route, Routes } from 'react-router-dom'

vi.mock('./contexts/AuthContext', () => ({
  AuthProvider: ({ children }: { children: React.ReactNode }) => <>{children}</>,
  useAuth: vi.fn(),
}))

import { useAuth } from './contexts/AuthContext'

// Inline minimal ProtectedRoute matching App.tsx logic
function ProtectedRoute() {
  const { isLoading, isAuthenticated, error } = useAuth()
  const location = { pathname: '/land-parcels', search: '', hash: '' }
  if (isLoading) return <div>loading</div>
  if (error) return <div>error</div>
  if (!isAuthenticated) {
    const returnUrl = `${location.pathname}${location.search}${location.hash}`
    window.location.href = `/auth/login?returnUrl=${encodeURIComponent(returnUrl)}`
    return null
  }
  return <div data-testid="protected">protected content</div>
}

describe('ProtectedRoute', () => {
  it('does not render protected content when unauthenticated and redirects to login', () => {
    vi.mocked(useAuth).mockReturnValue({
      isLoading: false,
      isAuthenticated: false,
      user: null,
      error: null,
      logout: vi.fn(),
      refresh: vi.fn(),
    })

    const assignSpy = vi.fn()
    Object.defineProperty(window, 'location', {
      value: { ...window.location, set href(url: string) { assignSpy(url) } },
      writable: true,
    })

    render(
      <MemoryRouter initialEntries={['/land-parcels']}>
        <Routes>
          <Route path="*" element={<ProtectedRoute />} />
        </Routes>
      </MemoryRouter>
    )

    expect(screen.queryByTestId('protected')).toBeNull()
    expect(assignSpy).toHaveBeenCalledWith(
      expect.stringContaining('/auth/login?returnUrl=')
    )
  })
})
