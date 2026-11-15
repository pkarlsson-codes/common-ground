import { useState } from 'react'
import type { ImprovementRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useWithdrawImprovementRequest } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  request: ImprovementRequestResponse
  onClose: () => void
}

/** Records that the homeowner dropped the work, before the decision or after approval and before completion. */
export default function WithdrawImprovementRequestPanel({ request, onClose }: Props) {
  const today = trustToday()
  const [reason, setReason] = useState('')
  const [withdrawnOn, setWithdrawnOn] = useState<CalendarDate | null>(today)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useWithdrawImprovementRequest()
  const { success } = useToast()
  const valid = reason.trim() !== '' && !!withdrawnOn && withdrawnOn >= (request.receivedOn ?? withdrawnOn)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ request, withdrawnOn: withdrawnOn!, reason })
      success('Request withdrawn')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to withdraw the request.'))
    }
  }

  return (
    <SlideOver
      title="Withdraw Improvement Request"
      subtitle={request.description}
      onClose={onClose}
      isDirty={reason !== '' || withdrawnOn !== today}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="withdraw-improvement-request-form"
            disabled={mutation.isPending || !valid}
          >
            {mutation.isPending ? 'Withdrawing…' : 'Withdraw'}
          </button>
        </>
      }
    >
      <form id="withdraw-improvement-request-form" onSubmit={handleSubmit}>
        {request.approvedOn && (
          <p style={{ fontSize: 13, color: 'var(--warning)', marginBottom: 14 }}>
            The approved credit will no longer count. Work done later needs a new request.
          </p>
        )}
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-withdrawn-on">Withdrawn On</label>
            <DateInput
              id="improvement-withdrawn-on"
              className="form-input"
              value={withdrawnOn}
              min={request.receivedOn}
              max={today}
              onChange={setWithdrawnOn}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="improvement-withdrawn-reason">Reason</label>
            <textarea
              id="improvement-withdrawn-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="The homeowner decided to wait until next year."
              required
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
