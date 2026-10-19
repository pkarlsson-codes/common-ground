import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useRecordMortgagePrequalification } from '../../hooks/applicantSelection'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type FormState = {
  lenderName: string
  approvedAmount: number
  expiryDate: string
}

const emptyForm = (): FormState => ({
  lenderName: '',
  approvedAmount: 0,
  expiryDate: '',
})

type Props = {
  householdId: string
  onClose: () => void
}

export default function RecordMortgagePrequalificationPanel({ householdId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordMortgagePrequalification()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        householdId,
        request: {
          lenderName: form.lenderName,
          approvedAmount: Number(form.approvedAmount),
          expiryDate: form.expiryDate,
        },
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
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
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
            <input
              className="form-input"
              type="date"
              value={form.expiryDate}
              onChange={e => setForm(f => ({ ...f, expiryDate: e.target.value }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
