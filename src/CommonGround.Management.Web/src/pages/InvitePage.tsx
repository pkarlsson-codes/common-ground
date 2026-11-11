import { useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import ApiProblemAlert from '../components/ApiProblemAlert'
import { useAuth } from '../contexts/AuthContext'
import { useAcceptInvitation, useInvitationPreview } from '../hooks/team'
import { getApiProblem, type ApiProblemDetails } from '../lib/getErrorMessage'

const cardStyle = {
  padding: '42px 44px',
  background: 'var(--surface)',
  border: '1px solid var(--border)',
  borderRadius: '12px',
  maxWidth: '440px',
  width: '100%',
  display: 'flex',
  flexDirection: 'column',
  gap: '18px',
} as const

const eyebrowStyle = {
  fontFamily: 'var(--font-mono)',
  fontSize: '9px',
  letterSpacing: '0.22em',
  textTransform: 'uppercase',
  color: 'var(--amber)',
} as const

const headingStyle = { margin: 0, fontFamily: 'var(--font-display)', fontSize: '30px', lineHeight: 1.15, color: 'var(--text-bright)' }
const textStyle = { margin: 0, fontSize: '13px', color: 'var(--text-dim)', lineHeight: 1.5 }

/** Where an invite link lands: shows the trust it joins and lets the signed-in user join it. */
export default function InvitePage() {
  const { token = '' } = useParams()
  const { user } = useAuth()

  return (
    <div style={{ minHeight: '100vh', display: 'flex', alignItems: 'center', justifyContent: 'center', background: 'var(--bg)', padding: '24px' }}>
      <div style={cardStyle}>
        <div style={eyebrowStyle}>Invite</div>
        {user?.hasTrust ? (
          <>
            <h1 style={headingStyle}>You already belong to a trust.</h1>
            <p style={textStyle}>An account can be in one trust. To join another, sign in with a different account.</p>
            <Link to="/" className="btn btn-primary">Go to Today</Link>
          </>
        ) : (
          <JoinTrust token={token} />
        )}
      </div>
    </div>
  )
}

function JoinTrust({ token }: { token: string }) {
  const preview = useInvitationPreview(token)
  const accept = useAcceptInvitation()
  const { refresh } = useAuth()
  const navigate = useNavigate()
  const [problem, setProblem] = useState<ApiProblemDetails | null>(null)

  if (preview.isLoading) return <div className="skeleton-rows" role="status" aria-label="Loading"><div className="skeleton-row" /></div>

  if (preview.isError || !preview.data) {
    return (
      <>
        <h1 style={headingStyle}>This invite can't be used</h1>
        <p style={textStyle}>This invite link is no longer valid. Ask a member of the trust for a new one.</p>
      </>
    )
  }

  const join = async () => {
    setProblem(null)
    try {
      await accept.mutateAsync(token)
      await refresh()
      navigate('/', { replace: true })
    } catch (err) {
      setProblem(await getApiProblem(err, 'Could not join the trust. Please try again.'))
    }
  }

  return (
    <>
      <h1 style={headingStyle}>Join {preview.data.trustName}</h1>
      <p style={textStyle}>This invite was made for {preview.data.email}.</p>
      <ApiProblemAlert problem={problem} />
      <button className="btn btn-primary" disabled={accept.isPending} onClick={() => { void join() }}>
        {accept.isPending ? 'Joining…' : 'Join Trust'}
      </button>
    </>
  )
}
