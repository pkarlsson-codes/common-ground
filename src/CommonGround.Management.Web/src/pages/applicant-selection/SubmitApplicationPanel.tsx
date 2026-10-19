import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { useSubmitApplication } from '../../hooks/applicantSelection'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatPhoneInput, isValidPhone, toE164 } from '../../lib/phone'

type MemberForm = {
  name: string
  email: string
  phone: string
}

type FormState = {
  members: MemberForm[]
  primaryIndex: number
  annualIncome: number
  accessibilityRequired: boolean
}

const emptyMember = (): MemberForm => ({ name: '', email: '', phone: '' })

const emptyForm = (): FormState => ({
  members: [emptyMember()],
  primaryIndex: 0,
  annualIncome: 0,
  accessibilityRequired: false,
})

type Props = {
  onClose: () => void
}

export default function SubmitApplicationPanel({ onClose }: Props) {
  const [form, setForm] = useState<FormState>(emptyForm)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const [phoneErrors, setPhoneErrors] = useState<Record<number, string>>({})
  const submitMutation = useSubmitApplication()
  const { success } = useToast()

  const updateMember = (index: number, patch: Partial<MemberForm>) =>
    setForm(f => ({ ...f, members: f.members.map((m, i) => (i === index ? { ...m, ...patch } : m)) }))

  const addMember = () => setForm(f => ({ ...f, members: [...f.members, emptyMember()] }))

  const removeMember = (index: number) => {
    setPhoneErrors({})
    setForm(f => ({
      ...f,
      members: f.members.filter((_, i) => i !== index),
      primaryIndex: f.primaryIndex === index ? 0 : f.primaryIndex > index ? f.primaryIndex - 1 : f.primaryIndex,
    }))
  }

  const isComplete = form.members.every(m => m.name && m.email && m.phone)

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)

    const errors: Record<number, string> = {}
    form.members.forEach((m, i) => {
      if (!isValidPhone(m.phone)) errors[i] = 'Enter a valid phone number.'
    })
    setPhoneErrors(errors)
    if (Object.keys(errors).length > 0) return

    try {
      const result = await submitMutation.mutateAsync({
        members: form.members.map((m, i) => ({
          name: m.name,
          email: m.email,
          phone: toE164(m.phone),
          isPrimary: i === form.primaryIndex,
        })),
        annualIncome: Number(form.annualIncome),
        requiresAccessibility: form.accessibilityRequired,
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
      subtitle="Register a household and submit its application"
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button
            className="btn btn-primary"
            type="submit"
            form="submit-homeowner-application-form"
            disabled={submitMutation.isPending || !isComplete}
          >
            {submitMutation.isPending ? 'Submitting…' : 'Submit Application'}
          </button>
        </>
      }
    >
      <form id="submit-homeowner-application-form" onSubmit={handleSubmit}>
        <div className="detail-section">
          <div className="detail-section__title">Household Members</div>
          {form.members.map((member, index) => (
            <div className="form-grid" key={index} style={{ marginTop: 10 }}>
              <div className="form-field full">
                <label className="form-label">Full Name</label>
                <input
                  className="form-input"
                  type="text"
                  value={member.name}
                  onChange={e => updateMember(index, { name: e.target.value })}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Email</label>
                <input
                  className="form-input"
                  type="email"
                  value={member.email}
                  onChange={e => updateMember(index, { email: e.target.value })}
                  required
                />
              </div>
              <div className="form-field">
                <label className="form-label">Phone</label>
                <input
                  className="form-input"
                  type="tel"
                  value={member.phone}
                  onChange={e => {
                    const next = formatPhoneInput(e.target.value, member.phone)
                    setPhoneErrors(errs => ({ ...errs, [index]: '' }))
                    updateMember(index, { phone: next })
                  }}
                  placeholder="e.g. (202) 555-1234"
                  required
                />
                {phoneErrors[index] && <div className="form-error">{phoneErrors[index]}</div>}
              </div>
              <div className="form-field full" style={{ display: 'flex', alignItems: 'center', gap: 12 }}>
                <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: 8, margin: 0 }}>
                  <input
                    type="radio"
                    name="primary-member"
                    checked={form.primaryIndex === index}
                    onChange={() => setForm(f => ({ ...f, primaryIndex: index }))}
                  />
                  Primary contact
                </label>
                {form.members.length > 1 && (
                  <button type="button" className="btn btn-ghost btn-sm" onClick={() => removeMember(index)}>
                    Remove
                  </button>
                )}
              </div>
            </div>
          ))}
          <button type="button" className="btn btn-ghost btn-sm" style={{ marginTop: 10 }} onClick={addMember}>
            Add Member
          </button>
        </div>

        <div className="form-grid" style={{ marginTop: 16 }}>
          <div className="form-field">
            <label className="form-label">Household Size</label>
            <div className="detail-field__value mono">{form.members.length}</div>
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
