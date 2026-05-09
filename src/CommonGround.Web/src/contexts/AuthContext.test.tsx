import { render, screen, waitFor } from '@testing-library/react'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { AuthProvider, useAuth } from './AuthContext'
import { AuthApi } from '../api'
import { ResponseError } from '../api/runtime'

vi.mock('../api', async (importOriginal) => {
  const mod = await importOriginal<typeof import('../api')>()
  return { ...mod, AuthApi: vi.fn() }
})

vi.mock('../lib/apiConfig', () => ({
  apiConfig: {},
}))

function TestConsumer() {
  const { isLoading, isAuthenticated, user, error } = useAuth()
  if (isLoading) return <div>loading</div>
  return (
    <div>
      <div data-testid="authenticated">{String(isAuthenticated)}</div>
      <div data-testid="userId">{user?.id ?? 'null'}</div>
      <div data-testid="error">{error ?? 'null'}</div>
    </div>
  )
}

function makeResponse(status: number): Response {
  return { status } as Response
}

describe('AuthContext', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('sets isAuthenticated and user when getCurrentUser returns valid data', async () => {
    const getCurrentUser = vi.fn().mockResolvedValue({
      id: 'user-1',
      username: 'alice',
      email: 'alice@example.com',
      trustId: 'trust-1',
      hasTrust: true,
    })
    vi.mocked(AuthApi).mockImplementation(function () {
      return { getCurrentUser } as never
    })

    render(<AuthProvider><TestConsumer /></AuthProvider>)
    await waitFor(() => expect(screen.queryByText('loading')).toBeNull())

    expect(screen.getByTestId('authenticated').textContent).toBe('true')
    expect(screen.getByTestId('userId').textContent).toBe('user-1')
    expect(screen.getByTestId('error').textContent).toBe('null')
  })

  it('sets isAuthenticated=false and error=null on 401', async () => {
    const getCurrentUser = vi.fn().mockRejectedValue(
      new ResponseError(makeResponse(401), 'Unauthorized')
    )
    vi.mocked(AuthApi).mockImplementation(function () {
      return { getCurrentUser } as never
    })

    render(<AuthProvider><TestConsumer /></AuthProvider>)
    await waitFor(() => expect(screen.queryByText('loading')).toBeNull())

    expect(screen.getByTestId('authenticated').textContent).toBe('false')
    expect(screen.getByTestId('userId').textContent).toBe('null')
    expect(screen.getByTestId('error').textContent).toBe('null')
  })

  it('sets error message on non-401 failure', async () => {
    const getCurrentUser = vi.fn().mockRejectedValue(new Error('Network error'))
    vi.mocked(AuthApi).mockImplementation(function () {
      return { getCurrentUser } as never
    })

    render(<AuthProvider><TestConsumer /></AuthProvider>)
    await waitFor(() => expect(screen.queryByText('loading')).toBeNull())

    expect(screen.getByTestId('authenticated').textContent).toBe('false')
    expect(screen.getByTestId('error').textContent).toBe(
      'Could not reach the server. Check your connection and reload.'
    )
  })
})
