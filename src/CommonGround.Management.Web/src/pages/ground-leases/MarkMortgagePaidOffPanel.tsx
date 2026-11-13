import { useState } from 'react'
import type { PermittedMortgageDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useMarkPermittedMortgagePaidOff } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  groundLeaseId: string
  mortgage: PermittedMortgageDto
  onClose: () => void
}

/** Marks a mortgage paid off without a refinance: the term ended, or the homeowner paid it early. */
export default function MarkMortgagePaidOffPanel({ groundLeaseId, mortgage, onClose }: Props) {
  const [paidOffOn, setPaidOffOn] = useState<CalendarDate | null>(null)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useMarkPermittedMortgagePaidOff()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!paidOffOn) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId, mortgageId: mortgage.mortgageId!, paidOffOn })
      success('Mortgage marked paid off')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to mark the mortgage paid off.'))
    }
  }

  return (
    <SlideOver
      title="Mark Mortgage Paid Off"
      subtitle={`${mortgage.lender} · ${formatPrice(mortgage.originalPrincipal ?? 0)}`}
      onClose={onClose}
      isDirty={paidOffOn !== null}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="mark-mortgage-paid-off-form"
            disabled={mutation.isPending || !paidOffOn}
          >
            {mutation.isPending ? 'Saving…' : 'Mark Paid Off'}
          </button>
        </>
      }
    >
      <form id="mark-mortgage-paid-off-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The mortgage stays on the lease, greyed. It no longer counts toward debt on new requests or gets copies of notices. This can't be undone.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="mortgage-paid-off-on">Paid Off On</label>
            <DateInput
              id="mortgage-paid-off-on"
              className="form-input"
              value={paidOffOn}
              min={mortgage.permittedOn ?? undefined}
              max={trustToday()}
              onChange={setPaidOffOn}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
