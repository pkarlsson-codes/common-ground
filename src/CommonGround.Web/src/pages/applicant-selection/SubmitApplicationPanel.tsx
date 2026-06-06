import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useSubmitApplication } from '../../hooks/applicantSelection'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

type FormState = {
  applicantFullName: string
  applicantEmail: string
  applicantPhone: string
  householdSize: number
  annualIncome: number
  accessibilityRequired: boolean
}

const emptyForm = (): FormState => ({
  applicantFullName: '',
  applicantEmail: '',
  applicantPhone: '',
  householdSize: 1,
  annualIncome: 0,
  accessibilityRequired: false,
})

type Props = {
  onClose: () => void
}

export default function SubmitApplicationPanel({ onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const submitMutation = useSubmitApplication()
  const { success } = useToast()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    try {
      const result = await submitMutation.mutateAsync({
        applicantFullName: form.applicantFullName,
        applicantEmail: form.applicantEmail,
        applicantPhone: form.applicantPhone || undefined,
        householdSize: Number(form.householdSize),
        annualIncome: Number(form.annualIncome),
        accessibilityRequired: form.accessibilityRequired,
      })

      success(result?.applicationId ? `Application submitted (${result.applicationId.slice(0, 8)}...)` : 'Application submitted')
      onClose()
    } catch (error) {
      setProblem(await getApiProblem(error, 'Failed to submit application.'))
    }
  }

  return (
    <SlideOver
      title="Submit Application"
      subtitle="Create a new homeowner application"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="submit-homeowner-application-form"
            disabled={submitMutation.isPending || !form.applicantFullName || !form.applicantEmail}
          >
            {submitMutation.isPending ? 'Submitting…' : 'Submit Application'}
          </button>
        </>
      }
    >
      <form id="submit-homeowner-application-form" onSubmit={handleSubmit}>
        <div className="form-grid">
          <div className="form-field full">
            <label className="form-label">Applicant Full Name</label>
            <input
              className="form-input"
              type="text"
              value={form.applicantFullName}
              onChange={e => setForm(f => ({ ...f, applicantFullName: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Applicant Email</label>
            <input
              className="form-input"
              type="email"
              value={form.applicantEmail}
              onChange={e => setForm(f => ({ ...f, applicantEmail: e.target.value }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Applicant Phone</label>
            <input
              className="form-input"
              type="tel"
              value={form.applicantPhone}
              onChange={e => setForm(f => ({ ...f, applicantPhone: e.target.value }))}
              placeholder="Optional"
            />
          </div>
          <div className="form-field">
            <label className="form-label">Household Size</label>
            <input
              className="form-input"
              type="number"
              min="1"
              value={form.householdSize}
              onChange={e => setForm(f => ({ ...f, householdSize: parseInt(e.target.value, 10) || 1 }))}
              required
            />
          </div>
          <div className="form-field">
            <label className="form-label">Annual Income ($)</label>
            <input
              className="form-input"
              type="number"
              min="0"
              step="1"
              value={form.annualIncome}
              onChange={e => setForm(f => ({ ...f, annualIncome: parseFloat(e.target.value) || 0 }))}
              required
            />
          </div>
          <div className="form-field full">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
              <input
                type="checkbox"
                checked={form.accessibilityRequired}
                onChange={e => setForm(f => ({ ...f, accessibilityRequired: e.target.checked }))}
              />
              Accessibility accommodations required
            </label>
          </div>
        </div>

        <ApiProblemAlert problem={problem} />
      </form>
    </SlideOver>
  )
}
