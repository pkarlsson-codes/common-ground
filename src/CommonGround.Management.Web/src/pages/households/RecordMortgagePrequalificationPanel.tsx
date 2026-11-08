import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRecordMortgagePrequalification } from '../../hooks/households'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import type { CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

type FormState = {
  lenderName: string
  approvedAmount: number
  expiryDate: CalendarDate | null
}

const emptyForm = (): FormState => ({
  lenderName: '',
  approvedAmount: 0,
  expiryDate: null,
})

type Props = {
  householdId: string
  onClose: () => void
}

export default function RecordMortgagePrequalificationPanel({ householdId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordMortgagePrequalification(householdId)
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        lenderName: form.lenderName,
        approvedAmount: Number(form.approvedAmount),
        expiryDate: form.expiryDate ?? undefined,
      })
      success('Mortgage prequalification recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record mortgage prequalification.'))
    }
  }

  return (
    <SlideOver
      title="Record Mortgage Prequalification"
      subtitle="Capture lender prequalification details"
      onClose={onClose}
      isDirty={!!form.lenderName || form.approvedAmount !== 0 || form.expiryDate !== null}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-mortgage-prequalification-form"
            disabled={mutation.isPending || !form.lenderName || !form.approvedAmount || !form.expiryDate}
          >
            {mutation.isPending ? 'Recording…' : 'Record Prequalification'}
          </button>
        </>
      }
    >
      <form id="record-mortgage-prequalification-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Lender Name</label>
            <input
              className="form-input"
              type="text"
              value={form.lenderName}
              onChange={e => setForm(f => ({ ...f, lenderName: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Approved Amount ($)</label>
            <input
              className="form-input"
              type="number"
              min="1"
              step="1"
              value={form.approvedAmount}
              onChange={e => setForm(f => ({ ...f, approvedAmount: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Expiry Date</label>
            <DateInput
              className="form-input"
              value={form.expiryDate}
              onChange={v => setForm(f => ({ ...f, expiryDate: v }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
