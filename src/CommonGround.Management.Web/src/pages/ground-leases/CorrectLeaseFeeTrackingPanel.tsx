import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useCorrectLeaseFeeTracking } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate, type CalendarDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/** Corrects the day a lease's fees are tracked from and what was owed on it, such as after an import with the wrong date. */
export default function CorrectLeaseFeeTrackingPanel({ lease, onClose }: Props) {
  const fees = lease.leaseFees
  const leaseStart = lease.leaseStartDate!
  const initialTrackedFrom = fees?.feesTrackedFrom ?? leaseStart
  const initialBalance = fees?.openingBalance ?? 0
  const [trackedFrom, setTrackedFrom] = useState<CalendarDate>(initialTrackedFrom)
  const [openingBalance, setOpeningBalance] = useState(initialBalance)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useCorrectLeaseFeeTracking()
  const { success } = useToast()

  const beforeStart = !!trackedFrom && trackedFrom < leaseStart
  const changed = trackedFrom !== initialTrackedFrom || openingBalance !== initialBalance
  const valid = !!trackedFrom && !beforeStart && changed

  const handleSubmit = async () => {
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId: lease.id!, feesTrackedFrom: trackedFrom, openingBalance })
      success('Fee tracking corrected')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to correct the fee tracking.'))
    }
  }

  return (
    <SlideOver
      title="Correct Fee Tracking"
      subtitle={lease.homeownerFullName ?? undefined}
      onClose={onClose}
      isDirty={changed}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="correct-lease-fee-tracking-form"
            disabled={mutation.isPending || !valid}
          >
            {mutation.isPending ? 'Saving…' : 'Save'}
          </button>
        </>
      }
    >
      <form id="correct-lease-fee-tracking-form" onSubmit={e => { e.preventDefault(); handleSubmit() }}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          Months before this day aren't charged here. The amount owed is due on that day. A payment received before
          the day must be voided first.
        </p>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="correct-fees-tracked-from">Fees Tracked From</label>
            <DateInput
              id="correct-fees-tracked-from"
              className="form-input"
              value={trackedFrom}
              onChange={v => setTrackedFrom(v)}
              required
            />
            {beforeStart && (
              <div className="form-help" style={{ color: 'var(--red)' }}>
                Can't be before the lease start, {formatCalendarDate(leaseStart)}.
              </div>
            )}
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="correct-opening-balance">Owed on That Day ($)</label>
            <input
              id="correct-opening-balance"
              className="form-input"
              type="number"
              min="0"
              step="0.01"
              value={openingBalance}
              onChange={e => setOpeningBalance(Math.max(0, parseFloat(e.target.value) || 0))}
            />
            <div className="form-help">Unpaid fees and interest from before, if any</div>
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
