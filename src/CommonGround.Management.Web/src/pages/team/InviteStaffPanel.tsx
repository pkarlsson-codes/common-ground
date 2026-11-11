import { useState } from 'react'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import SlideOver from '../../components/SlideOver'
import { inviteLink, useCreateInvitation } from '../../hooks/team'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'

/** Makes an invite link for a colleague and shows it once, to copy. */
export default function InviteStaffPanel({ onClose }: { onClose: () => void }) {
  const [email, setEmail] = useState('')
  const [link, setLink] = useState<string | null>(null)
  const [copied, setCopied] = useState(false)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)
  const mutation = useCreateInvitation()

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault()
    setProblem(null)
    try {
      const created = await mutation.mutateAsync(email.trim())
      setLink(inviteLink(created.token!))
    } catch (err) {
      setProblem(await getApiProblem(err, 'Could not create the invite.'))
    }
  }

  const copy = async () => {
    try {
      await navigator.clipboard.writeText(link!)
      setCopied(true)
    } catch {
      // The link stays on screen to copy by hand.
    }
  }

  return (
    <SlideOver
      title="Invite Staff"
      onClose={onClose}
      isDirty={!link && email.trim() !== ''}
      footer={link ? (
        <button type="button" className="btn btn-primary" onClick={onClose}>Done</button>
      ) : (
        <>
          <button type="button" className="btn btn-ghost" onClick={onClose}>Cancel</button>
          <button type="submit" form="invite-staff-form" className="btn btn-primary" disabled={mutation.isPending || email.trim() === ''}>
            {mutation.isPending ? 'Creating…' : 'Create Invite Link'}
          </button>
        </>
      )}
    >
      {link ? (
        <div className="detail-section">
          <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 12 }}>
            Send this link to {email.trim()}. It works once and expires in 7 days. It is shown only now; if it is lost,
            cancel the invite and make a new one.
          </p>
          <div className="form-field full">
            <label className="form-label" htmlFor="invite-link">Invite Link</label>
            <input id="invite-link" className="form-input" readOnly value={link} onFocus={e => e.target.select()} />
          </div>
          <button type="button" className="btn btn-ghost btn-sm" style={{ marginTop: 8 }} onClick={() => { void copy() }}>
            {copied ? 'Copied' : 'Copy Link'}
          </button>
        </div>
      ) : (
        <form id="invite-staff-form" onSubmit={handleSubmit}>
          <ApiProblemAlert problem={problem} />
          <p style={{ fontSize: 13, color: 'var(--text-secondary)', marginBottom: 12 }}>
            The app makes a link for you to send in your own email. Anyone in the trust can invite.
          </p>
          <div className="form-grid">
            <div className="form-field full">
              <label className="form-label" htmlFor="invite-email">Email</label>
              <input
                id="invite-email"
                className="form-input"
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
                required
                autoFocus
              />
            </div>
          </div>
        </form>
      )}
    </SlideOver>
  )
}
