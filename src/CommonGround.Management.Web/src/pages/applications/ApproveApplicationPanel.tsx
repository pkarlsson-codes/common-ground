import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useApproveApplication } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { localToday } from '../../lib/dates'

type FormState = {
  approvedDate: string
  notes: string
}

const emptyForm = (): FormState => ({
  approvedDate: localToday(),
  notes: '',
})

type Props = {
  applicationId: string
  onClose: () => void
}

export default function ApproveApplicationPanel({ applicationId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useApproveApplication()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId,
        request: {
          approvedDate: form.approvedDate,
          notes: form.notes || undefined,
        },
      })
      success('Application approved')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to approve application.'))
    }
  }

  return (
    <SlideOver
      title="Approve Application"
      subtitle="Confirm this household is eligible"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="approve-application-form"
            disabled={mutation.isPending || !form.approvedDate}
          >
            {mutation.isPending ? 'Approving…' : 'Approve'}
          </button>
        </>
      }
    >
      <form id="approve-application-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label">Approved Date</label>
            <input
              className="form-input"
              type="date"
              value={form.approvedDate}
              onChange={e => setForm(f => ({ ...f, approvedDate: e.target.value }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Notes</label>
            <textarea
              className="form-textarea"
              value={form.notes}
              onChange={e => setForm(f => ({ ...f, notes: e.target.value }))}
              placeholder="Optional internal notes..."
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
