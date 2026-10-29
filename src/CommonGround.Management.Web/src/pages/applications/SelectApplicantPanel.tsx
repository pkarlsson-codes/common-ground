import { useState } from 'react'
import { HomeownerApplicationsApi, GroundLeaseTemplatesApi } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import EntityCombobox from '../../components/EntityCombobox'
import SlideOver from '../../components/SlideOver'
import { useSelectApplicant } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { apiConfig } from '../../lib/apiConfig'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

const homeownerApplicationsApi = new HomeownerApplicationsApi(apiConfig)
const groundLeaseTemplatesApi = new GroundLeaseTemplatesApi(apiConfig)

type FormState = {
  applicationId: string
  groundLeaseTemplateId: string
  purchasePrice: number
  selectedDate: CalendarDate
}

const emptyForm = (): FormState => ({
  applicationId: '',
  groundLeaseTemplateId: '',
  purchasePrice: 0,
  selectedDate: trustToday(),
})

type Props = {
  homeId: string
  onClose: () => void
}

export default function SelectApplicantPanel({ homeId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useSelectApplicant()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId: form.applicationId,
        request: {
          homeId,
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
      subtitle="Finalize applicant selection for this home"
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
              || !form.applicationId
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
            <label className="form-label">Applicant</label>
            <EntityCombobox
              value={form.applicationId || null}
              onChange={id => setForm(f => ({ ...f, applicationId: id ?? '' }))}
              fetchOptions={async q => {
                const result = await homeownerApplicationsApi.getHomeCandidates({ homeId })
                const options = (result ?? []).map(c => ({
                  id: c.id!,
                  label: [c.primaryContact?.name, c.primaryContact?.email].filter(Boolean).join(' — ') || c.id!,
                }))
                const term = q.trim().toLowerCase()
                return term ? options.filter(o => o.label.toLowerCase().includes(term)) : options
              }}
              placeholder="Search approved applicants…"
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Ground Lease Template</label>
            <EntityCombobox
              value={form.groundLeaseTemplateId || null}
              onChange={id => setForm(f => ({ ...f, groundLeaseTemplateId: id ?? '' }))}
              fetchOptions={async q => {
                const result = await groundLeaseTemplatesApi.getGroundLeaseTemplates({
                  search: q,
                  status: 'Active',
                  pageSize: 10,
                })
                return (result.items ?? []).map(t => ({
                  id: t.id!,
                  label: t.templateName ?? t.id!,
                }))
              }}
              placeholder="Search templates…"
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
            <DateInput
              className="form-input"
              value={form.selectedDate}
              onChange={v => setForm(f => ({ ...f, selectedDate: v }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
