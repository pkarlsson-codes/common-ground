import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConfirmActionModal from '../../components/ConfirmActionModal'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useWaiveLeaseFeeInterest } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/** Waives late-fee interest owed today, all of it by default, such as for a household in hardship. */
export default function WaiveLeaseFeeInterestPanel({ lease, onClose }: Props) {
  const interestDue = lease.leaseFees?.statement?.interestDue ?? 0
  const [amount, setAmount] = useState(interestDue)
  const [reason, setReason] = useState('')
  const [confirming, setConfirming] = useState(false)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useWaiveLeaseFeeInterest()
  const { success } = useToast()

  const tooMuch = amount > interestDue
  const valid = amount > 0 && !tooMuch && reason.trim() !== ''

  const handleConfirm = async () => {
    setConfirming(false)
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId: lease.id!, amount, reason })
      success('Interest waived')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to waive the interest.'))
    }
  }

  return (
    <SlideOver
      title="Waive Interest"
      subtitle={`${lease.homeownerFullName} · ${formatPrice(interestDue)} interest owed`}
      onClose={onClose}
      isDirty={reason !== '' || amount !== interestDue}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="waive-lease-fee-interest-form"
            disabled={mutation.isPending || !valid}
          >
            {mutation.isPending ? 'Waiving…' : 'Waive Interest'}
          </button>
        </>
      }
    >
      <form id="waive-lease-fee-interest-form" onSubmit={e => { e.preventDefault(); setConfirming(true) }}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The waived amount comes off the balance. Interest on fees that stay unpaid keeps adding up.
        </p>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="waive-amount">Amount ($)</label>
            <input
              id="waive-amount"
              className="form-input"
              type="number"
              min="0.01"
              max={interestDue}
              step="0.01"
              value={amount}
              onChange={e => setAmount(Math.max(0, parseFloat(e.target.value) || 0))}
              required
            />
            {tooMuch && <div className="form-help" style={{ color: 'var(--red)' }}>Can't be more than {formatPrice(interestDue)}.</div>}
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="waive-reason">Reason</label>
            <textarea
              id="waive-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="Job loss, illness, payment delayed by the bank…"
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>

      <ConfirmActionModal
        open={confirming}
        title="Waive Interest"
        message={`Waive ${formatPrice(amount)} of interest? This can't be undone.`}
        confirmLabel="Waive Interest"
        onConfirm={handleConfirm}
        onCancel={() => setConfirming(false)}
      />
    </SlideOver>
  )
}
