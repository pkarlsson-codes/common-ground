import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useDenyApplication } from '../../hooks/applicantSelection'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type FormState = {
  reason: string
  deniedDate: string
}

const emptyForm = (): FormState => ({
  reason: '',
  deniedDate: new Date().toISOString().slice(0, 10),
})

type Props = {
  applicationId: string
  onClose: () => void
}

export default function DenyApplicationPanel({ applicationId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useDenyApplication()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
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
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
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
            <input
              className="form-input"
              type="date"
              value={form.deniedDate}
              onChange={e => setForm(f => ({ ...f, deniedDate: e.target.value }))}
              required
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
