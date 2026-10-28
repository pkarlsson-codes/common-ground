import { useState } from 'react'
import type { HouseholdMemberDto } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import {
  useAddHouseholdMember,
  useRemoveHouseholdMember,
  useUpdateHouseholdMember,
  type HouseholdMemberInput,
} from '../../hooks/households'
import { useToast } from '../../hooks/useToast'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import { formatPhoneDisplay, formatPhoneInput, isValidPhone, toE164 } from '../../lib/phone'

type Props = {
  householdId: string
  members: HouseholdMemberDto[]
}

// Which row is open: one member's edit form or remove confirmation, or the add form.
type Mode = { kind: 'edit' | 'remove'; memberId: string } | { kind: 'add' } | null

export default function HouseholdMembersSection({ householdId, members }: Props) {
  const [mode, setMode] = useState<Mode>(null)
  const addMember = useAddHouseholdMember(householdId)
  const updateMember = useUpdateHouseholdMember(householdId)
  const { success } = useToast()

  const close = () => setMode(null)

  return (
    <div className="detail-section" style={{ marginTop: 16 }}>
      <div className="detail-section__title">Household Members</div>
      <div className="detail-grid" style={{ marginTop: 10 }}>
        {members.map(member => {
          const memberId = member.id ?? ''
          const isOpen = mode !== null && mode.kind !== 'add' && mode.memberId === memberId

          if (isOpen && mode.kind === 'edit') {
            return (
              <div className="detail-field full-span" key={memberId}>
                <MemberForm
                  title={`Edit ${member.name}`}
                  initial={{ name: member.name ?? '', email: member.email ?? '', phone: formatPhoneDisplay(member.phone) ?? '' }}
                  submitLabel="Save Member"
                  pending={updateMember.isPending}
                  onCancel={close}
                  onSubmit={async input => {
                    await updateMember.mutateAsync({ memberId, ...input })
                    success('Member updated')
                    close()
                  }}
                />
              </div>
            )
          }

          return (
            <div className="detail-field full-span" key={memberId}>
              <div className="detail-field__label">{member.isPrimary ? 'Primary Contact' : 'Member'}</div>
              <div style={{ display: 'flex', alignItems: 'center', gap: 8 }}>
                <div className="detail-field__value" style={{ flex: 1 }}>
                  {[member.name, member.email, formatPhoneDisplay(member.phone)].filter(Boolean).join(' · ')}
                </div>
                <button
                  className="btn btn-ghost btn-sm"
                  type="button"
                  aria-label={`Edit ${member.name}`}
                  onClick={() => setMode({ kind: 'edit', memberId })}
                >
                  Edit
                </button>
                {!member.isPrimary && (
                  <button
                    className="btn btn-ghost btn-sm"
                    type="button"
                    aria-label={`Remove ${member.name}`}
                    onClick={() => setMode({ kind: 'remove', memberId })}
                  >
                    Remove
                  </button>
                )}
              </div>
              {isOpen && mode.kind === 'remove' && (
                <RemoveConfirmation householdId={householdId} member={member} onDone={close} />
              )}
            </div>
          )
        })}
      </div>

      {mode?.kind === 'add' ? (
        <div style={{ marginTop: 10 }}>
          <MemberForm
            title="New Member"
            initial={{ name: '', email: '', phone: '' }}
            submitLabel="Add Member"
            pending={addMember.isPending}
            onCancel={close}
            onSubmit={async input => {
              await addMember.mutateAsync(input)
              success('Member added')
              close()
            }}
          />
        </div>
      ) : (
        <button
          className="btn btn-ghost btn-sm"
          type="button"
          style={{ marginTop: 10 }}
          onClick={() => setMode({ kind: 'add' })}
        >
          Add Member
        </button>
      )}
    </div>
  )
}

function RemoveConfirmation({ householdId, member, onDone }: {
  householdId: string
  member: HouseholdMemberDto
  onDone: () => void
}) {
  const removeMember = useRemoveHouseholdMember(householdId)
  const { success } = useToast()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const handleRemove = async () => {
    setProblem(null)
    try {
      await removeMember.mutateAsync(member.id ?? '')
      success('Member removed')
      onDone()
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to remove member.'))
    }
  }

  return (
    <div style={{ marginTop: 8 }}>
      <div className="detail-field__value">Remove {member.name} from the household?</div>
      <ApiProblemAlert problem={problem} />
      <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
        <button
          className="btn btn-danger btn-sm"
          type="button"
          onClick={() => { void handleRemove() }}
          disabled={removeMember.isPending}
        >
          {removeMember.isPending ? 'Removing…' : 'Confirm Remove'}
        </button>
        <button className="btn btn-ghost btn-sm" type="button" onClick={onDone}>Cancel</button>
      </div>
    </div>
  )
}

function MemberForm({ title, initial, submitLabel, pending, onCancel, onSubmit }: {
  title: string
  initial: HouseholdMemberInput
  submitLabel: string
  pending: boolean
  onCancel: () => void
  onSubmit: (input: HouseholdMemberInput) => Promise<void>
}) {
  const [form, setForm] = useState(initial)
  const [phoneError, setPhoneError] = useState('')
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const isComplete = Boolean(form.name.trim() && form.email.trim() && form.phone.trim())

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    if (!isValidPhone(form.phone)) {
      setPhoneError('Enter a valid phone number.')
      return
    }
    try {
      await onSubmit({ name: form.name.trim(), email: form.email.trim(), phone: toE164(form.phone)! })
    } catch (err) {
      setProblem(await getApiProblem(err, 'Failed to save member.'))
    }
  }

  return (
    <form aria-label={title} onSubmit={e => { void handleSubmit(e) }}>
      <div className="form-grid">
        <div className="form-field full">
          <label className="form-label">Full Name</label>
          <input
            className="form-input"
            type="text"
            value={form.name}
            onChange={e => setForm(f => ({ ...f, name: e.target.value }))}
            required
          />
        </div>
        <div className="form-field">
          <label className="form-label">Email</label>
          <input
            className="form-input"
            type="email"
            value={form.email}
            onChange={e => setForm(f => ({ ...f, email: e.target.value }))}
            required
          />
        </div>
        <div className="form-field">
          <label className="form-label">Phone</label>
          <input
            className="form-input"
            type="tel"
            value={form.phone}
            onChange={e => {
              const next = formatPhoneInput(e.target.value, form.phone)
              setPhoneError('')
              setForm(f => ({ ...f, phone: next }))
            }}
            placeholder="e.g. (202) 555-1234"
            required
          />
          {phoneError && <div className="form-error">{phoneError}</div>}
        </div>
      </div>
      <ApiProblemAlert problem={problem} />
      <div style={{ display: 'flex', gap: 8, marginTop: 8 }}>
        <button className="btn btn-primary btn-sm" type="submit" disabled={pending || !isComplete}>
          {pending ? 'Saving…' : submitLabel}
        </button>
        <button className="btn btn-ghost btn-sm" type="button" onClick={onCancel}>Cancel</button>
      </div>
    </form>
  )
}
