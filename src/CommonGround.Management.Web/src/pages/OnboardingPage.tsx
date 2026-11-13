import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { useAuth } from '../contexts/AuthContext'
import { useDemoSandbox, useStartDemoSandbox } from '../hooks/demo'
import { getErrorMessage } from '../lib/getErrorMessage'

const cardStyle = {
  padding: '36px 40px',
  background: 'var(--surface)',
  border: '1px solid var(--border)',
  borderRadius: '12px',
  display: 'flex',
  flexDirection: 'column',
  gap: '20px',
} as const

const eyebrowStyle = {
  fontFamily: 'var(--font-mono)',
  fontSize: '9px',
  letterSpacing: '0.22em',
  textTransform: 'uppercase',
  color: 'var(--text-dim)',
  marginBottom: '8px',
} as const

const primaryStyle = {
  display: 'inline-flex',
  justifyContent: 'center',
  padding: '10px 20px',
  background: 'var(--amber)',
  color: 'var(--bg)',
  border: 'none',
  borderRadius: '6px',
  fontFamily: 'var(--font-body)',
  fontSize: '13.5px',
  fontWeight: 600,
  letterSpacing: '0.02em',
  textDecoration: 'none',
  cursor: 'pointer',
} as const

const secondaryStyle = {
  ...primaryStyle,
  background: 'transparent',
  color: 'var(--text-bright)',
  border: '1px solid var(--border)',
  fontWeight: 500,
} as const

const textStyle = { margin: 0, fontSize: '14px', color: 'var(--text-bright)', lineHeight: 1.5 } as const

/** Create a trust, or on a demo server, try a sample trust first. */
function NextStep() {
  const sandboxQuery = useDemoSandbox()
  if (sandboxQuery.isLoading) return <div className="skeleton-row" role="status" aria-label="Loading" />
  // Not a demo server, or the demo check failed: creating a trust still works.
  if (!sandboxQuery.data) return <CreateTrust />
  return <SampleTrust status={sandboxQuery.data.sandbox?.status} />
}

function CreateTrust() {
  return (
    <>
      <div>
        <div style={eyebrowStyle}>New trust</div>
        <p style={textStyle}>Starting a new Community Land Trust? Create one here and invite your team.</p>
      </div>
      <Link to='/signup' style={primaryStyle}>Create a trust</Link>
    </>
  )
}

function SampleTrust({ status }: { status?: string }) {
  const start = useStartDemoSandbox()
  const { refresh } = useAuth()
  const [error, setError] = useState<string | null>(null)

  // Once ready, the gateway has put the trust on the session; reloading the user leaves Onboarding for Today.
  useEffect(() => {
    if (status === 'Ready') void refresh()
  }, [status, refresh])

  async function onStart() {
    setError(null)
    try {
      await start.mutateAsync()
    } catch (startError) {
      setError(await getErrorMessage(startError, 'Could not start the sample trust. Please try again.'))
    }
  }

  const filling = start.isPending || status === 'Seeding' || status === 'Ready'
  const failed = status === 'Failed' && !start.isPending

  return (
    <>
      <div>
        <div style={eyebrowStyle}>Sample trust</div>
        <p style={textStyle}>
          Look around a trust that already holds a few years of homes, households, ground leases and lease fees.
          It is deleted on a set date, which a banner in the trust shows.
        </p>
      </div>
      {filling ? (
        <p role='status' style={{ ...textStyle, color: 'var(--text-secondary)' }}>
          Filling your sample trust with a few years of records… This usually takes less than a minute.
        </p>
      ) : (
        <>
          {failed && !error && <p role='alert' style={{ ...textStyle, color: 'var(--red)' }}>Could not set up the sample trust.</p>}
          {error && <p role='alert' style={{ ...textStyle, color: 'var(--red)' }}>{error}</p>}
          <button type='button' onClick={onStart} style={primaryStyle}>
            {failed ? 'Try again' : 'Try it with sample data'}
          </button>
          <Link to='/signup' style={secondaryStyle}>Create an empty trust</Link>
        </>
      )}
    </>
  )
}

export default function OnboardingPage() {
  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'var(--bg)',
        padding: '24px',
      }}
    >
      <div
        style={{
          display: 'grid',
          gridTemplateColumns: '1fr 1fr',
          gap: '64px',
          maxWidth: '900px',
          width: '100%',
          alignItems: 'center',
        }}
      >
        {/* Left: explanation */}
        <div style={{ display: 'flex', flexDirection: 'column', gap: '16px' }}>
          <div
            style={{
              fontFamily: 'var(--font-mono)',
              fontSize: '9px',
              letterSpacing: '0.22em',
              textTransform: 'uppercase',
              color: 'var(--amber)',
            }}
          >
            CommonGround
          </div>
          <h1
            style={{
              margin: 0,
              fontFamily: 'var(--font-display)',
              fontSize: '34px',
              lineHeight: 1.1,
              color: 'var(--text-bright)',
            }}
          >
            You're not part of a trust yet
          </h1>
          <p style={{ margin: 0, fontSize: '14px', color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            CommonGround is organized around Community Land Trusts. To work in an existing trust, you need an
            invite link from one of its members. Otherwise, you can create your own trust.
          </p>
          <p style={{ margin: 0, fontSize: '13px', color: 'var(--text-dim)', lineHeight: 1.5 }}>
            Ask a member of your trust to invite you from its Team page, then open the link they send you.
          </p>
        </div>

        {/* Right: what the user can do next */}
        <div style={cardStyle}>
          <NextStep />
        </div>
      </div>
    </div>
  )
}
