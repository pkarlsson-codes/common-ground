import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useRecordHousingCounseling } from '../../hooks/households'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

type FormState = {
  certificationNumber: string
  completedDate: CalendarDate
  counselorOrganization: string
}

const emptyForm = (): FormState => ({
  certificationNumber: '',
  completedDate: trustToday(),
  counselorOrganization: '',
})

type Props = {
  householdId: string
  onClose: () => void
}

export default function RecordHousingCounselingPanel({ householdId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useRecordHousingCounseling(householdId)
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        certificationNumber: form.certificationNumber,
        completedDate: form.completedDate,
        counselorOrganization: form.counselorOrganization,
      })
      success('Housing counseling recorded')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to record housing counseling.'))
    }
  }

  return (
    <SlideOver
      title="Record Housing Counseling"
      subtitle="Capture counseling completion details"
      onClose={onClose}
      isDirty={!!(form.certificationNumber || form.counselorOrganization) || form.completedDate !== emptyForm().completedDate}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="record-housing-counseling-form"
            disabled={mutation.isPending || !form.certificationNumber || !form.completedDate || !form.counselorOrganization}
          >
            {mutation.isPending ? 'Recording…' : 'Record Counseling'}
          </button>
        </>
      }
    >
      <form id="record-housing-counseling-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label">Certification Number</label>
            <input
              className="form-input"
              type="text"
              value={form.certificationNumber}
              onChange={e => setForm(f => ({ ...f, certificationNumber: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Completed Date</label>
            <DateInput
              className="form-input"
              value={form.completedDate}
              onChange={v => setForm(f => ({ ...f, completedDate: v }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Counselor Organization</label>
            <input
              className="form-input"
              type="text"
              value={form.counselorOrganization}
              onChange={e => setForm(f => ({ ...f, counselorOrganization: e.target.value }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
