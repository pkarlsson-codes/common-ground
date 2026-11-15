import { useState } from 'react'
import type { ImprovementRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useDenyImprovementRequest } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  request: ImprovementRequestResponse
  onClose: () => void
}

/** Refuses a request waiting for a decision. The reason is printed in the letter to the homeowner. */
export default function DenyImprovementRequestPanel({ request, onClose }: Props) {
  const today = trustToday()
  const [reason, setReason] = useState('')
  const [deniedOn, setDeniedOn] = useState<CalendarDate | null>(today)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useDenyImprovementRequest()
  const { success } = useToast()
  const valid = reason.trim() !== '' && !!deniedOn && deniedOn >= (request.receivedOn ?? deniedOn)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ request, deniedOn: deniedOn!, reason })
      success('Request denied')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to deny the request.'))
    }
  }

  return (
    <SlideOver
      title="Deny Improvement Request"
      subtitle={request.description}
      onClose={onClose}
      isDirty={reason !== '' || deniedOn !== today}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="deny-improvement-request-form"
            disabled={mutation.isPending || !valid}
          >
            {mutation.isPending ? 'Denying…' : 'Deny'}
          </button>
        </>
      }
    >
      <form id="deny-improvement-request-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The reason is printed in the letter to the homeowner.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-denied-on">Denied On</label>
            <DateInput
              id="improvement-denied-on"
              className="form-input"
              value={deniedOn}
              min={request.receivedOn}
              onChange={setDeniedOn}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="improvement-denial-reason">Reason</label>
            <textarea
              id="improvement-denial-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="The addition would cross the setback on the east side."
              required
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
