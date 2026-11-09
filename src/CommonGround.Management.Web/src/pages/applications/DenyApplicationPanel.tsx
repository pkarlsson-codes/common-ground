import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConfirmActionModal from '../../components/ConfirmActionModal'
import SlideOver from '../../components/SlideOver'
import SlideOverCancelButton from '../../components/SlideOverCancelButton'
import { useDenyApplication } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

type FormState = {
  reason: string
  deniedDate: CalendarDate
}

const emptyForm = (): FormState => ({
  reason: '',
  deniedDate: trustToday(),
})

type Props = {
  applicationId: string
  onClose: () => void
}

export default function DenyApplicationPanel({ applicationId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const [confirming, setConfirming] = useState(false)
  const mutation = useDenyApplication()
  const { success } = useToast()

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault()
    setConfirming(true)
  }

  const handleConfirm = async () => {
    setConfirming(false)
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId,
        request: {
          reason: form.reason,
          deniedDate: form.deniedDate,
        },
      })
      success('Application denied')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to deny application.'))
    }
  }

  return (
    <SlideOver
      title="Deny Application"
      subtitle="Record final application denial"
      onClose={onClose}
      isDirty={!!form.reason || form.deniedDate !== emptyForm().deniedDate}
      footer={
        <>
          <SlideOverCancelButton />
          <button
            className="btn btn-primary"
            type="submit"
            form="deny-application-form"
            disabled={mutation.isPending || !form.reason || !form.deniedDate}
          >
            {mutation.isPending ? 'Submitting…' : 'Deny Application'}
          </button>
        </>
      }
    >
      <form id="deny-application-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Reason</label>
            <textarea
              className="form-textarea"
              value={form.reason}
              onChange={e => setForm(f => ({ ...f, reason: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Denied Date</label>
            <DateInput
              className="form-input"
              value={form.deniedDate}
              onChange={v => setForm(f => ({ ...f, deniedDate: v }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>

      <ConfirmActionModal
        open={confirming}
        title="Deny Application"
        message="Deny this application? This action cannot be undone."
        confirmLabel="Deny Application"
        confirmTone="danger"
        onConfirm={handleConfirm}
        onCancel={() => setConfirming(false)}
      />
    </SlideOver>
  )
}
