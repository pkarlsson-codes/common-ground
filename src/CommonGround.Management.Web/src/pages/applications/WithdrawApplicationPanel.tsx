import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useWithdrawApplication } from '../../hooks/applications'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { trustToday, type CalendarDate } from '../../lib/calendarDate'
import { DateInput } from '../../components/DateInput'

type FormState = {
  withdrawnDate: CalendarDate
  reason: string
}

const emptyForm = (): FormState => ({
  withdrawnDate: trustToday(),
  reason: '',
})

type Props = {
  applicationId: string
  onClose: () => void
}

export default function WithdrawApplicationPanel({ applicationId, onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useWithdrawApplication()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      await mutation.mutateAsync({
        applicationId,
        request: {
          withdrawnDate: form.withdrawnDate,
          reason: form.reason || undefined,
        },
      })
      success('Application withdrawn')
      onClose()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to withdraw application.'))
    }
  }

  return (
    <SlideOver
      title="Withdraw Application"
      subtitle="Withdraw this application from the pipeline"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="withdraw-application-form"
            disabled={mutation.isPending || !form.withdrawnDate}
          >
            {mutation.isPending ? 'Withdrawing…' : 'Withdraw'}
          </button>
        </>
      }
    >
      <form id="withdraw-application-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field">
            <label className="form-label">Withdrawn Date</label>
            <DateInput
              className="form-input"
              value={form.withdrawnDate}
              onChange={v => setForm(f => ({ ...f, withdrawnDate: v }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label">Reason</label>
            <textarea
              className="form-textarea"
              value={form.reason}
              onChange={e => setForm(f => ({ ...f, reason: e.target.value }))}
              placeholder="Optional"
            />
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
