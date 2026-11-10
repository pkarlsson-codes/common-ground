import { useState } from 'react'
import type { LeaseFeePaymentDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConfirmActionModal from '../../components/ConfirmActionModal'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { PAYMENT_METHOD_LABELS, useVoidLeaseFeePayment } from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { formatCalendarDate } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  groundLeaseId: string
  payment: LeaseFeePaymentDto
  onClose: () => void
}

/** Voids a payment recorded by mistake, so it no longer counts toward the balance. */
export default function VoidLeaseFeePaymentPanel({ groundLeaseId, payment, onClose }: Props) {
  const [reason, setReason] = useState('')
  const [confirming, setConfirming] = useState(false)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useVoidLeaseFeePayment()
  const { success } = useToast()

  const summary = [
    payment.receivedDate ? formatCalendarDate(payment.receivedDate) : null,
    formatPrice(payment.amount ?? 0),
    PAYMENT_METHOD_LABELS[payment.method ?? ''] ?? payment.method,
  ].filter(Boolean).join(' · ')

  const handleConfirm = async () => {
    setConfirming(false)
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId, paymentId: payment.paymentId!, reason })
      success('Payment voided')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to void the payment.'))
    }
  }

  return (
    <SlideOver
      title="Void Payment"
      subtitle={summary}
      onClose={onClose}
      isDirty={reason !== ''}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-secondary"
            style={{ color: 'var(--red)', borderColor: 'var(--danger-surface)' }}
            type="submit"
            form="void-lease-fee-payment-form"
            disabled={mutation.isPending || !reason.trim()}
          >
            {mutation.isPending ? 'Voiding…' : 'Void Payment'}
          </button>
        </>
      }
    >
      <form id="void-lease-fee-payment-form" onSubmit={e => { e.preventDefault(); setConfirming(true) }}>
        <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 14 }}>
          The payment stays in the history marked Voided, and no longer counts toward the balance.
        </p>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label" htmlFor="void-reason">Reason</label>
            <textarea
              id="void-reason"
              className="form-textarea"
              value={reason}
              onChange={e => setReason(e.target.value)}
              placeholder="Check bounced, recorded on the wrong lease, wrong amount…"
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>

      <ConfirmActionModal
        open={confirming}
        title="Void Payment"
        message="Void this payment? This can't be undone. To correct an amount, record the payment again."
        confirmLabel="Void Payment"
        confirmTone="danger"
        onConfirm={handleConfirm}
        onCancel={() => setConfirming(false)}
      />
    </SlideOver>
  )
}
