import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useSelectApplicant } from '../../hooks/applicantSelection'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type FormState = {
  homeId: string
  landParcelId: string
  groundLeaseTemplateId: string
  purchasePrice: number
  selectedDate: string
}

const emptyForm = (): FormState => ({
  homeId: '',
  landParcelId: '',
  groundLeaseTemplateId: '',
  purchasePrice: 0,
  selectedDate: new Date().toISOString().slice(0, 10),
})

type Props = {
  applicationId: string
  onClose: () => void
}

export default function SelectApplicantPanel({ applicationId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSelectApplicant()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId,
        request: {
          homeId: form.homeId,
          landParcelId: form.landParcelId,
          groundLeaseTemplateId: form.groundLeaseTemplateId,
          purchasePrice: Number(form.purchasePrice),
          selectedDate: form.selectedDate,
        },
      })
      success('Applicant selected')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to select applicant.'))
    }
  }

  return (
    <SlideOver
      title="Select Applicant"
      subtitle="Finalize applicant selection for a property"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="select-applicant-form"
            disabled={
              mutation.isPending
              || !form.homeId
              || !form.landParcelId
              || !form.groundLeaseTemplateId
              || !form.purchasePrice
              || !form.selectedDate
            }
          >
            {mutation.isPending ? 'Selecting…' : 'Select Applicant'}
          </button>
        </>
      }
    >
      <form id="select-applicant-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Home ID</label>
            <input
              className="form-input"
              type="text"
              value={form.homeId}
              onChange={e => setForm(f => ({ ...f, homeId: e.target.value }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Land Parcel ID</label>
            <input
              className="form-input"
              type="text"
              value={form.landParcelId}
              onChange={e => setForm(f => ({ ...f, landParcelId: e.target.value }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Ground Lease Template ID</label>
            <input
              className="form-input"
              type="text"
              value={form.groundLeaseTemplateId}
              onChange={e => setForm(f => ({ ...f, groundLeaseTemplateId: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Purchase Price ($)</label>
            <input
              className="form-input"
              type="number"
              min="1"
              step="1"
              value={form.purchasePrice}
              onChange={e => setForm(f => ({ ...f, purchasePrice: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Selected Date</label>
            <input
              className="form-input"
              type="date"
              value={form.selectedDate}
              onChange={e => setForm(f => ({ ...f, selectedDate: e.target.value }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
