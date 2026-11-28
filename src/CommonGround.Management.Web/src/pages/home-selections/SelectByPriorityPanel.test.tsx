import { QueryClient, QueryClientProvider } from '@tanstack/react-query'
import { render, screen } from '@testing-library/react'
import { describe, expect, it, vi } from 'vitest'
import SelectByPriorityPanel from './SelectByPriorityPanel'

vi.mock('../../hooks/useToast', () => ({ useToast: () => ({ success: vi.fn() }) }))

const renderPanel = () =>
  render(
    <QueryClientProvider client={new QueryClient()}>
      <SelectByPriorityPanel homeId="home-1" entry={{ applicationId: 'app-1' }} assessedValue={null} onClose={vi.fn()} />
    </QueryClientProvider>,
  )

describe('SelectByPriorityPanel', () => {
  it('names every field by its label', () => {
    renderPanel()

    expect(screen.getByRole('group', { name: 'Priorities That Justify This Choice' })).toBeInTheDocument()
    expect(screen.getByRole('textbox', { name: 'Reason' })).toBeInTheDocument()
    expect(screen.getByRole('combobox', { name: 'Ground Lease Template' })).toBeInTheDocument()
    expect(screen.getByRole('spinbutton', { name: 'Purchase Price ($)' })).toBeInTheDocument()
    expect(screen.getByLabelText('Date of the Choice')).toBeInTheDocument()
  })
})
