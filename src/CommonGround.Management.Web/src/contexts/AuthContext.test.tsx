import { render, screen, waitFor } from '@testing-library/react'
import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { describe, it, expect, vi, beforeEach } from 'vitest'
import { AuthProvider, useAuth } from './AuthContext'
import { ResponseError } from '../api/runtime'

// hooks/auth builds its AuthApi once, at import, so every test sets this one function instead.
const { getCurrentUser } = vi.hoisted(() => ({ getCurrentUser: vi.fn() }))

vi.mock('../api', async (importOriginal) => {
  const mod = await importOriginal<typeof import('../api')>()
  return { ...mod, AuthApi: vi.fn(function () { return { getCurrentUser } }) }
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

function renderAuth() {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } })
  render(
    <QueryClientProvider client={queryClient}>
      <AuthProvider><TestConsumer /></AuthProvider>
    </QueryClientProvider>
  )
}

function makeResponse(status: number): Response {
  return { status } as Response
}

describe('AuthContext', () => {
  beforeEach(() => {
    getCurrentUser.mockReset()
  })

  it('sets isAuthenticated and user when getCurrentUser returns valid data', async () => {
    getCurrentUser.mockResolvedValue({
      id: 'user-1',
      username: 'alice',
      email: 'alice@example.com',
      trustId: 'trust-1',
      hasTrust: true,
    })

    renderAuth()
    await waitFor(() => expect(screen.queryByText('loading')).toBeNull())

    expect(screen.getByTestId('authenticated').textContent).toBe('true')
    expect(screen.getByTestId('userId').textContent).toBe('user-1')
    expect(screen.getByTestId('error').textContent).toBe('null')
  })

  it('sets isAuthenticated=false and error=null on 401', async () => {
    getCurrentUser.mockRejectedValue(
      new ResponseError(makeResponse(401), 'Unauthorized')
    )

    renderAuth()
    await waitFor(() => expect(screen.queryByText('loading')).toBeNull())

    expect(screen.getByTestId('authenticated').textContent).toBe('false')
    expect(screen.getByTestId('userId').textContent).toBe('null')
    expect(screen.getByTestId('error').textContent).toBe('null')
  })

  it('sets error message on non-401 failure', async () => {
    getCurrentUser.mockRejectedValue(new Error('Network error'))

    renderAuth()
    await waitFor(() => expect(screen.queryByText('loading')).toBeNull())

    expect(screen.getByTestId('authenticated').textContent).toBe('false')
    expect(screen.getByTestId('error').textContent).toBe(
      'Could not reach the server. Check your connection and reload.'
    )
  })
})
