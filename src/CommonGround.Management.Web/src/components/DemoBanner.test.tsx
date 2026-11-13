import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ResponseError } from '../api/runtime'
import { parseCalendarDate } from '../lib/calendarDate'
import DemoBanner from './DemoBanner'

const { getDemo } = vi.hoisted(() => ({ getDemo: vi.fn() }))
vi.mock('../api', async (importOriginal) => ({
  ...(await importOriginal<typeof import('../api')>()),
  DemoApi: vi.fn(function () { return { getDemo } }),
}))

const renderBanner = () =>
  render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <DemoBanner />
    </QueryClientProvider>,
  )

describe('DemoBanner', () => {
  beforeEach(() => { getDemo.mockReset() })

  it('shows the delete date and a feedback link in demo mode', async () => {
    getDemo.mockResolvedValue({ deletesOn: parseCalendarDate('2026-11-27'), feedbackUrl: 'https://forms.example.org/cg' })

    renderBanner()

    expect(await screen.findByText(/This trust is deleted on November 27, 2026\./)).toBeInTheDocument()
    const link = screen.getByRole('link', { name: 'Send feedback' })
    expect(link).toHaveAttribute('href', 'https://forms.example.org/cg')
    expect(link).toHaveAttribute('target', '_blank')
  })

  it('shows nothing when the server is not in demo mode', async () => {
    getDemo.mockRejectedValue(new ResponseError(new Response(null, { status: 404 })))

    const { container } = renderBanner()

    await vi.waitFor(() => expect(getDemo).toHaveBeenCalled())
    expect(container).toBeEmptyDOMElement()
  })
})
