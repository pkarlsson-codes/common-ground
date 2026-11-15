import { useState } from 'react'
import type { GroundLeaseResponse, ImprovementRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useApproveImprovementRequest } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  request: ImprovementRequestResponse
  lease: GroundLeaseResponse
  onClose: () => void
}

/**
 * Gives the trust's approval and names the credit it will count at resale. A credit past the lease's cap can still
 * be approved: the cap applies at resale, so the panel only says how much of it can count.
 */
export default function ApproveImprovementRequestPanel({ request, lease, onClose }: Props) {
  const today = trustToday()
  const cap = lease.improvementsCap ?? null
  const existing = lease.approvedImprovements ?? 0
  const estimated = request.estimatedCost ?? 0
  const room = cap == null ? 0 : Math.max(0, cap - existing)

  const [credit, setCredit] = useState(String(Math.min(estimated, room)))
  const [approvedOn, setApprovedOn] = useState<CalendarDate | null>(today)
  const [notes, setNotes] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useApproveImprovementRequest()
  const { success } = useToast()

  const amount = parseFloat(credit)
  const valid = !Number.isNaN(amount) && amount >= 0 && amount <= estimated
    && !!approvedOn && approvedOn >= (request.receivedOn ?? approvedOn)
  const counts = Math.min(amount, room)
  const overCap = cap != null && valid && amount > room

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!valid) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ request, approvedOn: approvedOn!, preApprovedCredit: amount, notes })
      success('Request approved')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to approve the request.'))
    }
  }

  return (
    <SlideOver
      title="Approve Improvement Request"
      subtitle={request.description}
      onClose={onClose}
      isDirty={notes !== '' || approvedOn !== today || credit !== String(Math.min(estimated, room))}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="approve-improvement-request-form"
            disabled={mutation.isPending || !valid}
          >
            {mutation.isPending ? 'Approving…' : 'Approve'}
          </button>
        </>
      }
    >
      <form id="approve-improvement-request-form" onSubmit={handleSubmit}>
        <div className="detail-grid" style={{ marginBottom: 14 }}>
          <div className="detail-field">
            <div className="detail-field__label">Estimated Cost</div>
            <div className="detail-field__value mono">{formatPrice(estimated)}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Improvements Cap</div>
            <div className="detail-field__value mono">{cap == null ? '—' : formatPrice(cap)}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Confirmed Improvements</div>
            <div className="detail-field__value mono">{formatPrice(existing)}</div>
          </div>
        </div>
        {cap == null && (
          <p style={{ fontSize: 13, color: 'var(--warning)', marginBottom: 14 }}>
            No cap on this lease, so no credit will count.
          </p>
        )}
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-credit">Pre-Approved Credit ($)</label>
            <input
              id="improvement-credit"
              className="form-input"
              type="number"
              min="0"
              max={estimated}
              step="0.01"
              value={credit}
              onChange={e => setCredit(e.target.value)}
              required
            />
            <p className="form-hint">The most the trust will count. Zero approves the work and counts nothing.</p>
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="improvement-approved-on">Approved On</label>
            <DateInput
              id="improvement-approved-on"
              className="form-input"
              value={approvedOn}
              min={request.receivedOn}
              onChange={setApprovedOn}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="improvement-approval-notes">Notes (optional)</label>
            <textarea
              id="improvement-approval-notes"
              className="form-textarea"
              value={notes}
              onChange={e => setNotes(e.target.value)}
              placeholder="Licensed contractor required."
            />
          </div>
        </div>
        {overCap && (
          <p role="status" style={{ fontSize: 13, color: 'var(--warning)', marginTop: 14 }}>
            Only {formatPrice(counts)} of this can count; the lease's cap is {formatPrice(cap)}. You can still approve it.
          </p>
        )}
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
