import { useState } from 'react'
import type { GroundLeaseResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import {
  PAYMENT_METHOD_LABELS,
  PAYMENT_METHODS,
  useRecordLeaseFeePayment,
  type PaymentMethod,
  type RecordLeaseFeePaymentForm,
} from '../../hooks/groundLeases'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatBalance } from './leaseFeeFormat'

interface Props {
  lease: GroundLeaseResponse
  onClose: () => void
}

/** Records a lease fee payment the trust received, prefilled with what is due. */
export default function RecordLeaseFeePaymentPanel({ lease, onClose }: Props) {
  const balance = lease.leaseFees?.statement?.balance ?? 0
  const suggested = balance > 0 ? balance : (lease.leaseFees?.monthlyFee ?? 0)
  const [form, setForm] = useState<RecordLeaseFeePaymentForm>(() => ({
    receivedDate: trustToday(),
    amount: suggested,
    method: 'Check',
    reference: '',
  }))
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordLeaseFeePayment()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ groundLeaseId: lease.id!, form })
      success('Payment recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the payment.'))
    }
  }

  return (
    <SlideOver
      title="Record Payment"
      subtitle={`${lease.homeownerFullName ?? 'Ground lease'} · ${formatBalance(balance)}`}
      onClose={onClose}
      isDirty={form.reference !== '' || form.amount !== suggested}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-lease-fee-payment-form"
            disabled={mutation.isPending || !form.receivedDate || form.amount <= 0}
          >
            {mutation.isPending ? 'Recording…' : 'Record Payment'}
          </button>
        </>
      }
    >
      <form id="record-lease-fee-payment-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="payment-received-date">Received Date</label>
            <DateInput
              id="payment-received-date"
              className="form-input"
              value={form.receivedDate}
              min={lease.leaseStartDate}
              max={trustToday()}
              onChange={v => setForm(f => ({ ...f, receivedDate: v }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="payment-amount">Amount ($)</label>
            <input
              id="payment-amount"
              className="form-input"
              type="number"
              min="0.01"
              step="0.01"
              value={form.amount || ''}
              onChange={e => setForm(f => ({ ...f, amount: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="payment-method">Method</label>
            <select
              id="payment-method"
              className="form-select"
              value={form.method}
              onChange={e => setForm(f => ({ ...f, method: e.target.value as PaymentMethod }))}
            >
              {PAYMENT_METHODS.map(m => <option key={m} value={m}>{PAYMENT_METHOD_LABELS[m]}</option>)}
            </select>
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="payment-reference">Reference</label>
            <input
              id="payment-reference"
              className="form-input"
              value={form.reference}
              onChange={e => setForm(f => ({ ...f, reference: e.target.value }))}
              placeholder="Check number, optional"
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
