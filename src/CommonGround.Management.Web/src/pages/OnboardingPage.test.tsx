import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ResponseError } from '../api/runtime'
import OnboardingPage from './OnboardingPage'

const { getCurrentSandbox, startSandbox, refresh } = vi.hoisted(() => ({
  getCurrentSandbox: vi.fn(),
  startSandbox: vi.fn(),
  refresh: vi.fn(),
}))
vi.mock('../api', async (importOriginal) => ({
  ...(await importOriginal<typeof import('../api')>()),
  DemoApi: vi.fn(function () { return { getCurrentSandbox, startSandbox } }),
}))
vi.mock('../contexts/AuthContext', () => ({ useAuth: () => ({ refresh }) }))

const renderPage = () =>
  render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <MemoryRouter>
        <OnboardingPage />
      </MemoryRouter>
    </QueryClientProvider>,
  )

const problem = (status: number, detail: string) =>
  new ResponseError(new Response(JSON.stringify({ detail }), { status, headers: { 'Content-Type': 'application/problem+json' } }))

describe('OnboardingPage', () => {
  beforeEach(() => {
    getCurrentSandbox.mockReset()
    startSandbox.mockReset()
    refresh.mockReset()
  })

  it('offers only Create a trust when the server is not in demo mode', async () => {
    getCurrentSandbox.mockRejectedValue(new ResponseError(new Response(null, { status: 404 })))

    renderPage()

    expect(await screen.findByRole('link', { name: 'Create a trust' })).toHaveAttribute('href', '/signup')
    expect(screen.queryByRole('button', { name: 'Try it with sample data' })).not.toBeInTheDocument()
  })

  it('offers sample data first and an empty trust second in demo mode', async () => {
    getCurrentSandbox.mockResolvedValue({ sandbox: null })

    renderPage()

    expect(await screen.findByRole('button', { name: 'Try it with sample data' })).toBeInTheDocument()
    expect(screen.getByRole('link', { name: 'Create an empty trust' })).toHaveAttribute('href', '/signup')
  })

  it('shows the wait text while filling and reloads the user once ready', async () => {
    getCurrentSandbox.mockResolvedValueOnce({ sandbox: null }).mockResolvedValue({ sandbox: { trustId: 't1', status: 'Ready' } })
    startSandbox.mockResolvedValue({ trustId: 't1', status: 'Seeding' })

    renderPage()
    await userEvent.click(await screen.findByRole('button', { name: 'Try it with sample data' }))

    expect(screen.getByRole('status')).toHaveTextContent('Filling your sample trust with a few years of records…')
    await vi.waitFor(() => expect(refresh).toHaveBeenCalledTimes(1), { timeout: 4000 })
  })

  it('resumes the wait when a sample trust is already being filled', async () => {
    getCurrentSandbox.mockResolvedValue({ sandbox: { trustId: 't1', status: 'Seeding' } })

    renderPage()

    expect(await screen.findByText(/Filling your sample trust/)).toBeInTheDocument()
    expect(screen.queryByRole('button')).not.toBeInTheDocument()
  })

  it('offers Try again when filling failed', async () => {
    getCurrentSandbox.mockResolvedValue({ sandbox: { trustId: 't1', status: 'Failed' } })
    startSandbox.mockResolvedValue({ trustId: 't2', status: 'Seeding' })

    renderPage()
    expect(await screen.findByRole('alert')).toHaveTextContent('Could not set up the sample trust.')
    await userEvent.click(screen.getByRole('button', { name: 'Try again' }))

    expect(startSandbox).toHaveBeenCalledTimes(1)
    expect(await screen.findByText(/Filling your sample trust/)).toBeInTheDocument()
  })

  it('says the demo is full and starts nothing', async () => {
    getCurrentSandbox.mockResolvedValue({ sandbox: null })
    startSandbox.mockRejectedValue(problem(422, 'The demo is full right now. Try again tomorrow.'))

    renderPage()
    await userEvent.click(await screen.findByRole('button', { name: 'Try it with sample data' }))

    expect(await screen.findByRole('alert')).toHaveTextContent('The demo is full right now. Try again tomorrow.')
    expect(screen.getByRole('button', { name: 'Try it with sample data' })).toBeInTheDocument()
  })
})
