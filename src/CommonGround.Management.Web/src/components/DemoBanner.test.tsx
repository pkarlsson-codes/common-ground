import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { MemoryRouter } from 'react-router-dom'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import { ResponseError } from '../api/runtime'
import { parseCalendarDate } from '../lib/calendarDate'
import DemoBanner from './DemoBanner'

const { getDemo, sendFeedback, getCurrentUser } = vi.hoisted(() => ({
  getDemo: vi.fn(),
  sendFeedback: vi.fn(),
  getCurrentUser: vi.fn(),
}))
vi.mock('../api', async (importOriginal) => ({
  ...(await importOriginal<typeof import('../api')>()),
  DemoApi: vi.fn(function () { return { getDemo, sendFeedback } }),
  AuthApi: vi.fn(function () { return { getCurrentUser } }),
}))

const renderBanner = () =>
  render(
    <QueryClientProvider client={new QueryClient({ defaultOptions: { queries: { retry: false } } })}>
      <MemoryRouter initialEntries={['/resales/resale-1']}>
        <DemoBanner />
      </MemoryRouter>
    </QueryClientProvider>,
  )

describe('DemoBanner', () => {
  beforeEach(() => {
    getDemo.mockReset()
    sendFeedback.mockReset()
    getCurrentUser.mockReset()
    getCurrentUser.mockResolvedValue({ id: 'u1', email: 'visitor@example.org', trustId: 't1', hasTrust: true })
  })

  it('shows the delete date and a feedback button in demo mode', async () => {
    getDemo.mockResolvedValue({ deletesOn: parseCalendarDate('2026-11-27') })

    renderBanner()

    expect(await screen.findByText(/This trust is deleted on November 27, 2026\./)).toBeInTheDocument()
    expect(screen.getByRole('button', { name: 'Send feedback' })).toBeInTheDocument()
  })

  it('sends the feedback with the page and permission to reply', async () => {
    getDemo.mockResolvedValue({ deletesOn: parseCalendarDate('2026-11-27') })
    sendFeedback.mockResolvedValue(null)
    const user = userEvent.setup()
    renderBanner()

    await user.click(await screen.findByRole('button', { name: 'Send feedback' }))
    await user.type(screen.getByLabelText('Feedback'), '  The price looks wrong  ')
    expect(await screen.findByLabelText('You can reply to me at visitor@example.org')).toBeChecked()
    await user.click(screen.getByRole('button', { name: 'Send' }))

    await vi.waitFor(() => expect(sendFeedback).toHaveBeenCalledWith({
      sendDemoFeedbackRequest: { message: 'The price looks wrong', page: '/resales/resale-1', mayReply: true },
    }))
  })

  it('sends no permission to reply when the box is unticked', async () => {
    getDemo.mockResolvedValue({ deletesOn: parseCalendarDate('2026-11-27') })
    sendFeedback.mockResolvedValue(null)
    const user = userEvent.setup()
    renderBanner()

    await user.click(await screen.findByRole('button', { name: 'Send feedback' }))
    await user.type(screen.getByLabelText('Feedback'), 'Hello')
    await user.click(await screen.findByLabelText('You can reply to me at visitor@example.org'))
    await user.click(screen.getByRole('button', { name: 'Send' }))

    await vi.waitFor(() => expect(sendFeedback).toHaveBeenCalledWith({
      sendDemoFeedbackRequest: { message: 'Hello', page: '/resales/resale-1', mayReply: false },
    }))
  })

  it('shows nothing when the server is not in demo mode', async () => {
    getDemo.mockRejectedValue(new ResponseError(new Response(null, { status: 404 })))

    const { container } = renderBanner()

    await vi.waitFor(() => expect(getDemo).toHaveBeenCalled())
    expect(container).toBeEmptyDOMElement()
  })
})
