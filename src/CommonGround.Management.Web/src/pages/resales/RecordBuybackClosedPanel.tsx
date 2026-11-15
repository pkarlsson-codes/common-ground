import { useState } from 'react'
import type { ResaleResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import { usePayoffStatement, useRecordBuybackClosed, type RecordBuybackClosedForm } from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  resale: ResaleResponse
  onClose: () => void
}

/** Records that the trust paid for and took the home, and what happens to the home next. */
export default function RecordBuybackClosedPanel({ resale, onClose }: Props) {
  const optionPrice = resale.purchaseOptionPrice!
  const [form, setForm] = useState<RecordBuybackClosedForm>(() => ({
    closingDate: trustToday(),
    pricePaid: optionPrice,
    notes: '',
    offerForSale: true,
    holdReason: '',
    feesPaidFromProceeds: 0,
  }))
  // The payment is dated today, so the amount offered is what the lease owes today. Staff can change it.
  const owed = usePayoffStatement(resale.id!, trustToday(), true)
  const [feesEdited, setFeesEdited] = useState(false)
  const feesPaid = feesEdited ? form.feesPaidFromProceeds : owed.data?.totalOwed ?? 0

  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordBuybackClosed()
  const { success } = useToast()

  const valid = form.pricePaid > 0 && form.pricePaid <= optionPrice && (form.offerForSale || form.holdReason.trim() !== '')

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ resaleId: resale.id!, form: { ...form, feesPaidFromProceeds: feesPaid } })
      success('Buyback closed')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record the closing.'))
    }
  }

  return (
    <SlideOver
      title="Record Closing"
      subtitle={resale.homeLabel ?? undefined}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button type="submit" form="record-buyback-closed-form" className="btn btn-primary" disabled={mutation.isPending || !valid}>
            {mutation.isPending ? 'Recording…' : 'Record Closing'}
          </button>
        </>
      }
    >
      <form id="record-buyback-closed-form" onSubmit={handleSubmit}>
        <div className="detail-grid" style={{ marginBottom: 16 }}>
          <div className="detail-field">
            <div className="detail-field__label">Purchase Option Price</div>
            <div className="detail-field__value mono">{formatPrice(optionPrice)}</div>
          </div>
        </div>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="buyback-closing-date">Closing Date</label>
            <DateInput
              id="buyback-closing-date"
              className="form-input"
              value={form.closingDate}
              min={resale.optionExercisedDate ?? undefined}
              max={trustToday()}
              onChange={v => setForm(f => ({ ...f, closingDate: v }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="buyback-price-paid">Purchase Price Paid ($)</label>
            <input
              id="buyback-price-paid"
              className="form-input"
              type="number"
              min="0"
              max={optionPrice}
              step="0.01"
              value={form.pricePaid || ''}
              onChange={e => setForm(f => ({ ...f, pricePaid: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label" htmlFor="buyback-fees-from-proceeds">Fees Paid from Proceeds ($)</label>
            <input
              id="buyback-fees-from-proceeds"
              className="form-input"
              type="number"
              min="0"
              step="0.01"
              value={feesPaid}
              onChange={e => {
                setFeesEdited(true)
                setForm(f => ({ ...f, feesPaidFromProceeds: Math.max(0, parseFloat(e.target.value) || 0) }))
              }}
            />
            <div className="form-help">Recorded on the lease as paid from sale proceeds. Zero for none.</div>
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="buyback-notes">Closing Notes (optional)</label>
            <textarea
              id="buyback-notes"
              className="form-textarea"
              value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="buyback-what-next">What Happens Next</label>
            <select
              id="buyback-what-next"
              className="form-select"
              value={form.offerForSale ? 'offer' : 'hold'}
              onChange={e => setForm(f => ({ ...f, offerForSale: e.target.value === 'offer' }))}
            >
              <option value="offer">Offer to the waiting list</option>
              <option value="hold">Hold for now</option>
            </select>
          </div>
          {!form.offerForSale && (
            <div className="form-field full">
              <label className="form-label" htmlFor="buyback-hold-reason">Hold Reason</label>
              <textarea
                id="buyback-hold-reason"
                className="form-textarea"
                value={form.holdReason}
                onChange={e => setForm(f => ({ ...f, holdReason: e.target.value }))}
                required
              />
            </div>
          )}
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
