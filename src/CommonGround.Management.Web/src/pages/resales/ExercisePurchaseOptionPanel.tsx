import { useState } from 'react'
import type { ResaleResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import { DateInput } from '../../components/DateInput'
import SlideOver from '../../components/SlideOver'
import { optionWindowLeft, useExercisePurchaseOption, type ExercisePurchaseOptionForm } from '../../hooks/resales'
import { useToast } from '../../hooks/useToast'
import { trustToday } from '../../lib/calendarDate'
import { formatPrice } from '../../lib/currency'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

interface Props {
  resale: ResaleResponse
  onClose: () => void
}

/** Records that the trust will buy the home at the Purchase Option Price. */
export default function ExercisePurchaseOptionPanel({ resale, onClose }: Props) {
  const [form, setForm] = useState<ExercisePurchaseOptionForm>(() => ({ exercisedDate: trustToday(), notes: '' }))
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useExercisePurchaseOption()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      await mutation.mutateAsync({ resaleId: resale.id!, form })
      success('Purchase option exercised')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to exercise the purchase option.'))
    }
  }

  return (
    <SlideOver
      title="Buy the Home"
      subtitle={resale.homeLabel ?? undefined}
      onClose={onClose}
      footer={
        <>
          <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button type="submit" form="exercise-purchase-option-form" className="btn btn-primary" disabled={mutation.isPending || !form.exercisedDate}>
            {mutation.isPending ? 'Recording…' : 'Exercise Option'}
          </button>
        </>
      }
    >
      <form id="exercise-purchase-option-form" onSubmit={handleSubmit}>
        <div className="detail-grid" style={{ marginBottom: 16 }}>
          <div className="detail-field">
            <div className="detail-field__label">Purchase Option Price</div>
            <div className="detail-field__value mono">{formatPrice(resale.purchaseOptionPrice!)}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Option Window End</div>
            <div className="detail-field__value mono">{resale.optionWindowEndsOn}</div>
          </div>
          <div className="detail-field">
            <div className="detail-field__label">Option Window</div>
            <div className="detail-field__value">{optionWindowLeft(resale.optionWindowEndsOn!)}</div>
          </div>
        </div>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label" htmlFor="resale-exercised-date">Exercised Date</label>
            <DateInput
              id="resale-exercised-date"
              className="form-input"
              value={form.exercisedDate}
              min={resale.appraisalDate ?? undefined}
              max={trustToday()}
              onChange={v => setForm(f => ({ ...f, exercisedDate: v }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" htmlFor="resale-exercised-notes">Notes (optional)</label>
            <textarea
              id="resale-exercised-notes"
              className="form-textarea"
              value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
            />
          </div>
        </div>
        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
