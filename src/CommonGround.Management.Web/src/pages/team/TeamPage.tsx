import { useState } from 'react'
import type { TrustInvitationResponse } from '../../api'
import ApiProblemAlert from '../../components/ApiProblemAlert'
import ConfirmActionModal from '../../components/ConfirmActionModal'
import PageEyebrow from '../../components/PageEyebrow'
import { useCancelInvitation, usePendingInvitations, useTrustMembers } from '../../hooks/team'
import { useToast } from '../../hooks/useToast'
import { formatInstantDate } from '../../lib/calendarDate'
import { getApiProblem, type ApiProblemDetails } from '../../lib/getErrorMessage'
import InviteStaffPanel from './InviteStaffPanel'

const dash = <span style={{ color: 'var(--text-dim)' }}>—</span>

/** The trust's members and the invites not yet used. */
export default function TeamPage() {
  const members = useTrustMembers()
  const invitations = usePendingInvitations()
  const [inviting, setInviting] = useState(false)
  const pending = invitations.data?.items ?? []

  return (
    <div className="page">
      <div className="page-header">
        <div>
          <PageEyebrow />
          <h1 className="page-header__title">Team</h1>
        </div>
        <button className="btn btn-primary" onClick={() => setInviting(true)}>Invite Staff</button>
      </div>

      <div className="detail-section">
        <div className="detail-section__title">Members</div>
        {members.isLoading && <div className="skeleton-rows"><div className="skeleton-row" /></div>}
        {members.isError && <div className="error-state"><div className="error-state__title">Could not load the members</div></div>}
        {members.data && (
          <table className="registry-table" aria-label="Members">
            <thead><tr><th>Name</th><th>Email</th></tr></thead>
            <tbody>
              {members.data.items?.map(m => (
                <tr key={m.userId}>
                  <td>{m.name ?? dash}</td>
                  <td>{m.email ?? dash}</td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {pending.length > 0 && (
        <div className="detail-section">
          <div className="detail-section__title">Pending Invites</div>
          <table className="registry-table" aria-label="Pending Invites">
            <thead><tr><th>Email</th><th>Invited By</th><th>Expires</th><th /></tr></thead>
            <tbody>
              {pending.map(i => <PendingInviteRow key={i.id} invitation={i} />)}
            </tbody>
          </table>
        </div>
      )}

      {inviting && <InviteStaffPanel onClose={() => setInviting(false)} />}
    </div>
  )
}

function PendingInviteRow({ invitation }: { invitation: TrustInvitationResponse }) {
  const cancel = useCancelInvitation()
  const { success } = useToast()
  const [confirming, setConfirming] = useState(false)
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  const onConfirm = async () => {
    setConfirming(false)
    setProblem(null)
    try {
      await cancel.mutateAsync(invitation.id!)
      success('Invite cancelled')
    } catch (err) {
      setProblem(await getApiProblem(err, 'Could not cancel the invite.'))
    }
  }

  return (
    <tr>
      <td>{invitation.email}</td>
      <td>{invitation.invitedByName ?? dash}</td>
      <td><span className="cell-mono">{invitation.expiresAt ? formatInstantDate(invitation.expiresAt) : ''}</span></td>
      <td style={{ textAlign: 'right' }}>
        <button className="btn btn-ghost btn-sm" disabled={cancel.isPending} onClick={() => setConfirming(true)}>
          Cancel
        </button>
        <ApiProblemAlert problem={problem} />
        <ConfirmActionModal
          open={confirming}
          title="Cancel this invite?"
          message={`The link sent to ${invitation.email} will stop working.`}
          confirmLabel="Cancel Invite"
          confirmTone="danger"
          onConfirm={() => { void onConfirm() }}
          onCancel={() => setConfirming(false)}
        />
      </td>
    </tr>
  )
}
