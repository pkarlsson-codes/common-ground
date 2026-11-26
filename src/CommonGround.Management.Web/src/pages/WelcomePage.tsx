const cardStyle = {
  padding: 'clamp(20px, 6vw, 36px) clamp(20px, 6vw, 40px)',
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

/** The landing page for a signed-out visitor on a demo server. */
export default function WelcomePage({ sandboxDays }: { sandboxDays?: number }) {
  return (
    <div
      style={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        background: 'var(--bg)',
        padding: '24px 16px',
      }}
    >
      <main
        style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fit, minmax(min(100%, 320px), 1fr))',
          gap: '48px 64px',
          maxWidth: '900px',
          width: '100%',
          alignItems: 'center',
        }}
      >
        {/* Left: what CommonGround is */}
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
            Try CommonGround with a sample trust
          </h1>
          <p style={{ margin: 0, fontSize: '14px', color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            CommonGround is free software for running a Community Land Trust: homes and land, households applying to
            buy, ground leases and lease fees, resales, stewardship and funder reports.
          </p>
          <p style={{ margin: 0, fontSize: '14px', color: 'var(--text-secondary)', lineHeight: 1.6 }}>
            It is still being built, and we want CLT staff to tell us what is missing or wrong before more is added.
            Every page in the demo has a link for sending feedback.
          </p>
        </div>

        {/* Right: what the demo holds and how to start */}
        <div style={cardStyle}>
          <div>
            <div style={eyebrowStyle}>The demo</div>
            <p style={textStyle}>
              Create an account, confirm your email, and you get your own sample trust with a few years of records:
              homes and land parcels, households and their applications, ground leases and lease fees, resales,
              stewardship cases, and last year's funder report. The sample trust is in Missoula, Montana. Change
              anything you like. No one else sees your trust, and it is deleted{' '}
              {sandboxDays ? `${sandboxDays} days after you start it.` : 'a set number of days after you start it.'}
            </p>
          </div>
          <p role='note' style={{ ...textStyle, color: 'var(--red)' }}>
            Don't enter real personal information. Everything on this server is sample data.
          </p>
          <a href='/auth/login?register=true' style={primaryStyle}>Try the demo</a>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '8px' }}>
            <a href='/auth/login' style={secondaryStyle}>Sign in</a>
            <p style={{ margin: 0, fontSize: '13px', color: 'var(--text-dim)', lineHeight: 1.5 }}>
              Already started a sample trust? Sign in to go back to it.
            </p>
          </div>
        </div>
      </main>
    </div>
  )
}
