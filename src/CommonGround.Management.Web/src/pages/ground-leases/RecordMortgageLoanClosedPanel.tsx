import { useState } from 'react'
import type { GroundLeaseResponse, MortgageRequestResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRecordMortgageLoanClosed } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate, trustToday, type CalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { mortgageRequestTitle } from './mortgageRequestLetter'

interface Props {
  request: MortgageRequestResponse
  lease: GroundLeaseResponse
  onClose: () => void
}

/** Records that an approved loan closed: it becomes a permitted mortgage, and the ones it paid off are marked. */
export default function RecordMortgageLoanClosedPanel({ request, lease, onClose }: Props) {
  const today = trustToday()
  const expected = request.expectedClosingOn
  const current = (lease.mortgages?.permitted ?? []).filter(m => !m.paidOffOn)
  const [closedOn, setClosedOn] = useState<CalendarDate | null>(expected && expected <= today ? expected : today)
  const [noticeAddress, setNoticeAddress] = useState('')
  const [paidOff, setPaidOff] = useState<Set<string>>(
    () => new Set(request.purpose === 'Refinance' ? current.map(m => m.mortgageId!) : []),
  )
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordMortgageLoanClosed()
  const { success } = useToast()

  const toggle = (id: string) => setPaidOff(s => {
    const next = new Set(s)
    if (next.has(id)) next.delete(id)
    else next.add(id)
    return next
  })

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    if (!closedOn) return
    setProblem(null)
    try {
      await mutation.mutateAsync({ request, closedOn, paidOffMortgageIds: [...paidOff], noticeAddress })
      success('Loan recorded closed')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the loan closed.'))
    }
  }

  return (
    <SlideOver
      title="Record Loan Closed"
      subtitle={mortgageRequestTitle(request)}
      onClose={onClose}
      isDirty={noticeAddress !== ''}
      footer={
        <>
          <SlideOverCancelButton />
          <button className="btn btn-primary" type="submit" form="record-mortgage-loan-closed-form" disabled={mutation.isPending || !closedOn}>
            {mutation.isPending ? 'Recording…' : 'Record Loan Closed'}
          </button>
        </>
      }
    >
      <form id="record-mortgage-loan-closed-form" onSubmit={handleSubmit}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The {formatPrice(request.principal ?? 0)} loan becomes a permitted mortgage on the lease, permitted on the
          approval date. This can't be undone.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="loan-closed-on">Closed On</label>
            <DateInput
              id="loan-closed-on"
              className="form-input"
              value={closedOn}
              min={request.decidedOn ?? undefined}
              max={today}
              onChange={setClosedOn}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="loan-closed-notice-address">Notice Address for {request.lender}</label>
            <textarea
              id="loan-closed-notice-address"
              className="form-textarea"
              value={noticeAddress}
              onChange={e => setNoticeAddress(e.target.value)}
              placeholder={'Loss Mitigation Dept.\n100 Main St\nSpringfield, IL 62701'}
              aria-describedby="loan-closed-notice-address-hint"
            />
            <p id="loan-closed-notice-address-hint" className="form-hint">
              Where the trust sends copies of notices of default. Optional; leave empty if not known yet.
            </p>
          </div>
          {current.length > 0 && (
            <fieldset className="form-field full" style={{ border: 0, padding: 0, margin: 0 }}>
              <legend className="form-label">Paid off by this loan</legend>
              {current.map(m => (
                <label key={m.mortgageId} style={{ display: 'flex', gap: 8, alignItems: 'center', fontSize: 13, padding: '4px 0' }}>
                  <input type="checkbox" checked={paidOff.has(m.mortgageId!)} onChange={() => toggle(m.mortgageId!)} />
                  {m.lender} · {formatPrice(m.originalPrincipal ?? 0)}
                  {m.permittedOn && <span style={{ color: 'var(--text-dim)' }}>permitted {formatCalendarDate(m.permittedOn)}</span>}
                </label>
              ))}
            </fieldset>
          )}
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
